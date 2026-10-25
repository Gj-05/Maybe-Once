using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
namespace OctoEarth.Core
{
    public sealed class WinningCondition
    { public string id,description,value; public bool passed; }
    public sealed class MissionSummary
    {
        public string status,scope,trajectoryFile,limitations;
        public double seconds,targetTimeSeconds,finalA_AU,finalE,temperatureMinimum,temperatureMaximum,materialKg,jetEnergyJ,solarInputJ,sourceFraction,energyError,momentumError,angularError,impulseError,reactionError,minimumSafetyFactor,maximumShade;
        public bool completed,partialStep,placeholderThresholds;
        public double finalEjectedKg,postTargetEjectedKg,materialHeadroomKg,branchStartSeconds,maximumEarthShade,maximumMirrorShade,maximumTideRatio,maximumFlux,minimumEarthPlumeClearance,minimumMoonPlumeClearance,maximumArrayJetPower,maximumMirrorJetPower,maximumRockStress,maximumArrayBendingStress,maximumMirrorBendingStress;
        public bool departureComplete;
        public WinningCondition[] conditions;
        public MissionFlag[] flags;
        public MissionConfiguration config;
    }
    public sealed class RecordedBenchmarks { public int schema; public BenchmarkResult[] results; }
    public sealed class FinalMissionBody { public string name; public double mass; public double[] position,velocity; }
    public sealed class FinalMissionState { public double time; public FinalMissionBody[] bodies; public bool partialStep; }
    public sealed class EnablerDeclaration
    { public string id,description; public double bound; public string units; }
    public sealed class MissionSimulation
    {
        static readonly double[] CheckpointYears={1,10,100,1000};
        public SystemState DiagnosticState => state.Clone();
        public MissionSimulation TargetCheckpoint {get;private set;}
        readonly MissionConfiguration c;
        readonly MissionThresholds limits;
        readonly SystemState state;
        Ledger ledger;
        readonly MissionTargets targets;
        readonly MissionConstraintStepper stepper;
        readonly MissionRadiation radiation;
        readonly MissionMonitors monitors;
        EnergyBalanceClimate climate;
        SystemState lastValidCoastState;
        readonly int earth,sun,moon,tractor,array,mirror;
        double supplyLeft,electricalLeft,initialMaterial,targetTime=-1,initialArrayArea;
        double consumedPrefix,ejectedPrefix,targetEjected,branchStart;
        readonly double propellantReservePerStep;
        KahanSum solarEnergy,electricalUsed,jetEnergy,solarReflected,arrayHeat,thrusterHeat,unusedElectrical;
        public MissionSimulation(string dataPath,MissionConfiguration config,MissionThresholds thresholds)
        {
            c=config; c.Validate(); limits=thresholds;
            var initial=GravityTractor.Planetary(dataPath,c.Tractor());
            var list=new List<Body>(initial.Bodies); var ep=initial.Bodies[initial.Index("Earth")].Position; var sp=initial.Bodies[initial.Index("Sun")].Position;
            double nominalForce=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear).Force*c.ThrustMargin;
            propellantReservePerStep=nominalForce*c.Dt/(c.ExhaustSpeed*Math.Cos(Units.Radians(c.CantDegrees)))*1.1;
            double flux=Insolation.Flux(c.ArrayRadiusAU*Constants.AU),cos=Math.Cos(Units.Radians(c.CantDegrees));
            double requested=nominalForce*c.ExhaustSpeed/(2*cos*c.ThrusterEfficiency);
            double skPerArea=flux*(1+c.ArrayReflectivity)/Constants.C*c.ExhaustSpeed/(2*cos*c.ThrusterEfficiency);
            initialArrayArea=(1+c.PowerReserve)*requested/(flux*c.ArrayEfficiency-skPerArea);
            if(!(initialArrayArea>0) || initialArrayArea>c.MaxArrayArea) throw new InvalidOperationException("Initial collector cannot supply its own station keeping and tractor within array cap.");
            list.Add(new Body("Array",initialArrayArea*c.ArrayDensity,sp+new Vector3d(c.ArrayRadiusAU*Constants.AU,0,0),new Vector3d()));
            list.Add(new Body("Mirror",10,ep-new Vector3d(c.MirrorDistance,0,0),new Vector3d())); // 10kg seed material, counted in E1.
            state=new SystemState(list.ToArray()); earth=state.Index("Earth"); sun=state.Index("Sun"); moon=state.Index("Moon"); tractor=state.Index("Tractor"); array=state.Index("Array"); mirror=state.Index("Mirror");
            targets=new MissionTargets(state,c); targets.Begin(state);
            foreach(int index in targets.Controlled) { targets.Get(state,index,out var r,out var v); state.Bodies[index].Position=r; state.Bodies[index].Velocity=v; }
            state.Barycenter();
            initialMaterial=state.Bodies[array].Mass+state.Bodies[mirror].Mass;
            if(initialMaterial>c.MaterialCap) throw new InvalidOperationException("Initial infrastructure exceeds E1 material cap");
            var absent=state.Clone(); absent.Bodies[array].Mass=0; absent.Bodies[mirror].Mass=0;
            var p=state.Bodies[array].Velocity*state.Bodies[array].Mass+state.Bodies[mirror].Velocity*state.Bodies[mirror].Mass;
            var l=Vector3d.Cross(state.Bodies[array].Position,state.Bodies[array].Velocity*state.Bodies[array].Mass)+Vector3d.Cross(state.Bodies[mirror].Position,state.Bodies[mirror].Velocity*state.Bodies[mirror].Mass);
            var history=new LedgerLine{Name="EnablerSource"}; history.Add(-initialMaterial,-1*p,-1*l,-(Ledger.Energy(state)-Ledger.Energy(absent)));
            ledger=new Ledger(state,0,new[]{history});
            stepper=new MissionConstraintStepper(state,c,targets);
            radiation=new MissionRadiation(state,c,initialArrayArea,c.EnableMirror?10/c.MirrorDensity:0);
            climate=new EnergyBalanceClimate(c.InitialTemperature,c.MixedLayerDepth);
            monitors=new MissionMonitors(state,c,limits);
            stepper.SetResourceCheck(CheckDraw);
        }
        public MissionSimulation Fork(string policy)
        {
            if(targetTime<0) throw new InvalidOperationException("Post-target comparison requires an actually reached target state");
            var config=MissionConfigurationIO.Clone(c); config.PostTargetPolicy=policy; return new MissionSimulation(this,config);
        }
        public static MissionSimulation RestartRecordedTarget(string dataPath,MissionConfiguration config,MissionThresholds thresholds,string csv)
        {
            var model=new MissionSimulation(dataPath,config,thresholds);
            using(var reader=File.OpenText(csv))
            {
                var h=reader.ReadLine().Split(','); var f=reader.ReadLine().Split(',');
                double Value(string name)=>double.Parse(f[Array.IndexOf(h,name)],CultureInfo.InvariantCulture);
                foreach(var body in model.state.Bodies)
                {
                    body.Position=new Vector3d(Value(body.Name+"_x"),Value(body.Name+"_y"),Value(body.Name+"_z"));
                    body.Velocity=new Vector3d(Value(body.Name+"_vx"),Value(body.Name+"_vy"),Value(body.Name+"_vz"));
                }
                model.radiation.ArrayArea=Value("array_area_m2"); model.radiation.MirrorArea=Value("mirror_area_m2");
                model.state.Bodies[model.array].Mass=model.radiation.ArrayArea*config.ArrayDensity; model.state.Bodies[model.mirror].Mass=model.radiation.MirrorArea*config.MirrorDensity;
                model.state.Time=Value("time_s"); model.branchStart=model.state.Time; model.targetTime=model.state.Time; model.targets.CutoffTime=model.state.Time;
                model.consumedPrefix=Value("material_kg"); model.ejectedPrefix=Value("ejected_kg"); model.targetEjected=model.ejectedPrefix;
                model.ledger=new Ledger(model.state); model.climate=new EnergyBalanceClimate(Value("temperature_K"),config.MixedLayerDepth);
                model.initialArrayArea=model.radiation.ArrayArea;
            }
            return model;
        }
        MissionSimulation(MissionSimulation parent,MissionConfiguration config)
        {
            c=config; limits=parent.limits; state=parent.state.Clone(); earth=state.Index("Earth"); sun=state.Index("Sun"); moon=state.Index("Moon"); tractor=state.Index("Tractor"); array=state.Index("Array"); mirror=state.Index("Mirror");
            consumedPrefix=parent.Materials; ejectedPrefix=parent.ejectedPrefix+parent.stepper.Ejected; targetEjected=parent.targetEjected;
            branchStart=state.Time; targetTime=parent.targetTime; initialArrayArea=parent.radiation.ArrayArea; propellantReservePerStep=parent.propellantReservePerStep;
            ledger=new Ledger(state); targets=new MissionTargets(state,c){CutoffTime=targetTime}; stepper=new MissionConstraintStepper(state,c,targets){TractorThrust=parent.stepper.TractorThrust};
            radiation=new MissionRadiation(state,c,parent.radiation.ArrayArea,parent.radiation.MirrorArea); climate=new EnergyBalanceClimate(parent.climate.Temperature,c.MixedLayerDepth);
            monitors=new MissionMonitors(state,c,limits); foreach(var f in parent.monitors.Flags.All) monitors.Flags.Add(f.firstTimeSeconds,f.code,f.cause,f.enabler);
            solarEnergy=parent.solarEnergy; electricalUsed=parent.electricalUsed; jetEnergy=parent.jetEnergy; solarReflected=parent.solarReflected; arrayHeat=parent.arrayHeat; thrusterHeat=parent.thrusterHeat; unusedElectrical=parent.unusedElectrical;
            stepper.SetResourceCheck(CheckDraw);
        }
        double Materials => consumedPrefix+(ledger.Lines.TryGetValue("EnablerSource",out var source)?-source.Mass:0);
        void CheckDraw(double mass)
        {
            if(Materials+mass>c.MaterialCap) throw new InvalidOperationException("MaterialCap: E1 has no remaining supplied mass");
            if(mass>supplyLeft) throw new InvalidOperationException("DeliveryRate: required propellant exceeds available E1 rate");
            double electrical=mass*c.ExhaustSpeed*c.ExhaustSpeed/(2*c.ThrusterEfficiency);
            if(electrical>electricalLeft) throw new InvalidOperationException("SolarPowerShortfall: physical hover impulse cannot be supplied by the actual collector");
            supplyLeft-=mass; electricalLeft-=electrical; electricalUsed.Add(electrical);
        }
        void Grow(int index,double mass)
        {
            if(!(mass>0)) return;
            mass=Math.Min(mass,Math.Min(supplyLeft,Math.Max(0,c.MaterialCap-Materials)));
            var body=state.Bodies[index]; var p=body.Velocity*mass;
            double energy=mass*body.Velocity.SqrMagnitude/2+Ledger.ParcelPotential(state,index,mass);
            body.Mass+=mass; ledger.Book("EnablerSource",-mass,-1*p,Vector3d.Cross(body.Position,-1*p),-energy); supplyLeft-=mass;
        }
        void Advance()
        {
            SystemState coastBefore=targets.TractorUnconstrained && c.PostTargetPolicy=="Coast"?state.Clone():null;
            radiation.Evaluate(state); supplyLeft=c.DeliveryRate*c.Dt;
            double reserveMass=propellantReservePerStep;
            double growthBudget=Math.Max(0,supplyLeft-reserveMass);
            double targetMirror=c.EnableMirror?Math.Min(c.MaxMirrorArea,radiation.MirrorRequiredArea):0;
            if(!double.IsInfinity(targetMirror)) Grow(mirror,Math.Min(growthBudget,Math.Max(0,targetMirror*c.MirrorDensity-state.Bodies[mirror].Mass)));
            radiation.MirrorArea=c.EnableMirror?state.Bodies[mirror].Mass/c.MirrorDensity:0;
            double tractorPower=stepper.StepJetEnergy>0?stepper.StepJetEnergy/c.Dt/c.ThrusterEfficiency:stepper.TractorThrust.Magnitude>0?stepper.TractorThrust.Magnitude*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees))*c.ThrusterEfficiency):radiation.ArrayArea*radiation.ArrayFlux*c.ArrayEfficiency/(1+c.PowerReserve);
            double targetArea=Math.Min(c.MaxArrayArea,Math.Max(initialArrayArea,tractorPower/(radiation.ArrayFlux*c.ArrayEfficiency)*(1+c.PowerReserve)));
            Grow(array,Math.Min(Math.Max(0,supplyLeft-reserveMass),Math.Max(0,targetArea*c.ArrayDensity-state.Bodies[array].Mass)));
            radiation.ArrayArea=state.Bodies[array].Mass/c.ArrayDensity; radiation.Evaluate(state);
            double incident=radiation.ArrayFlux*radiation.ArrayArea*c.Dt; solarEnergy.Add(incident);
            solarReflected.Add(incident*c.ArrayReflectivity); arrayHeat.Add(incident*Math.Max(0,1-c.ArrayReflectivity-c.ArrayEfficiency));
            electricalLeft=incident*c.ArrayEfficiency;
            stepper.Step(state,ledger,radiation,c.Dt);
            if(coastBefore!=null)
            {
                var old=coastBefore.Bodies[tractor].Position-coastBefore.Bodies[earth].Position;
                var current=state.Bodies[tractor].Position-state.Bodies[earth].Position; var delta=current-old;
                double fraction=delta.SqrMagnitude==0?0:Math.Max(0,Math.Min(1,-Vector3d.Dot(old,delta)/delta.SqrMagnitude));
                double surface=Constants.EarthRadius+Math.Pow(3*state.Bodies[tractor].Mass/(4*Math.PI*c.RockDensity),1.0/3);
                if((old+delta*fraction).Magnitude<surface) { lastValidCoastState=coastBefore; throw new InvalidOperationException("Collision.Tractor.Earth: swept surface crossing in last step; post-impact point-mass elements are invalid"); }
            }
            if((state.Bodies[tractor].Position-state.Bodies[earth].Position).Magnitude<Constants.EarthRadius+Math.Pow(3*state.Bodies[tractor].Mass/(4*Math.PI*c.RockDensity),1.0/3)) throw new InvalidOperationException("Collision.Tractor.Earth: physical surfaces intersect; point-mass continuation is invalid");
            jetEnergy.Add(stepper.StepJetEnergy); thrusterHeat.Add(stepper.StepJetEnergy*(1/c.ThrusterEfficiency-1)); unusedElectrical.Add(electricalLeft);
            climate.StepRadiation(c.Dt,radiation.VisibleFlux,radiation.InfraredFlux);
            monitors.Check(state,ledger,stepper,radiation,climate,Materials);
            if(targetTime<0 && monitors.A>=2.7*Constants.AU) { targetTime=state.Time; targets.CutoffTime=state.Time; targetEjected=ejectedPrefix+stepper.Ejected; TargetCheckpoint=Fork("Parking"); }
        }
        string Header()
        {
            var fields=new List<string>{"time_s","a_m","e","temperature_K","effective_flux_W_m2","thrust_N","ejected_kg","array_area_m2","mirror_area_m2","material_kg","relative_P","relative_L","relative_E","min_safety_factor","tide_ratio","moon_bound","plume_miss_m","shading_fraction","flag_count"};
            foreach(var body in state.Bodies) foreach(string field in new[]{"x","y","z","vx","vy","vz"}) fields.Add(body.Name+"_"+field);
            fields.AddRange(new[]{"tractor_mass_kg","tractor_distance_m","tractor_mdot_kg_s","tractor_ejected_kg","tractor_accel_along_earth_velocity_m_s2","tractor_da_dt_AU_yr","shade_earth","shade_mirror","direct_visible_W_m2","reflected_visible_W_m2","array_IR_W_m2","earth_array_distance_m","mirror_array_distance_m"});
            return string.Join(",",fields);
        }
        void WriteRow(StreamWriter file)
        {
            var values=new List<double>{state.Time,monitors.A,monitors.E,climate.Temperature,radiation.EarthFlux,stepper.TractorThrust.Magnitude,ejectedPrefix+stepper.Ejected,radiation.ArrayArea,radiation.MirrorArea,Materials,ledger.MaxMomentumError,ledger.MaxAngularMomentumError,ledger.MaxEnergyError,monitors.MinSafetyFactor,monitors.TideRatio,monitors.AllMoonBound?1:0,monitors.MinPlumeMiss,monitors.MaxShade,monitors.Flags.All.Length};
            foreach(var b in state.Bodies) values.AddRange(new[]{b.Position.X,b.Position.Y,b.Position.Z,b.Velocity.X,b.Velocity.Y,b.Velocity.Z});
            var eb=state.Bodies[earth]; var tb=state.Bodies[tractor]; var sb=state.Bodies[sun]; var mb=state.Bodies[moon];
            var delta=tb.Position-eb.Position; var acc=delta*(Constants.G*tb.Mass/Math.Pow(delta.Magnitude,3));
            var moonDelta=tb.Position-mb.Position; var moonAcc=moonDelta*(Constants.G*tb.Mass/Math.Pow(moonDelta.Magnitude,3));
            var sunDelta=tb.Position-sb.Position; var sunAcc=sunDelta*(Constants.G*tb.Mass/Math.Pow(sunDelta.Magnitude,3));
            var pairV=(eb.Velocity*eb.Mass+mb.Velocity*mb.Mass)/(eb.Mass+mb.Mass)-sb.Velocity;
            var pairAcc=(acc*eb.Mass+moonAcc*mb.Mass)/(eb.Mass+mb.Mass)-sunAcc;
            double rate=2*monitors.A*monitors.A/Constants.SolarMu*Vector3d.Dot(pairV,pairAcc)*Constants.JulianYear/Constants.AU;
            values.AddRange(new[]{tb.Mass,delta.Magnitude,stepper.TractorThrust.Magnitude/(c.ExhaustSpeed*Math.Cos(Units.Radians(c.CantDegrees))),stepper.Drives[0].TotalEjected,Vector3d.Dot(acc,(eb.Velocity-sb.Velocity).Normalized),rate});
            values.AddRange(new[]{radiation.ArrayShadeEarth,radiation.ArrayShadeMirror,radiation.DirectFlux,radiation.ReflectedFlux,radiation.InfraredFlux,(eb.Position-state.Bodies[array].Position).Magnitude,(state.Bodies[mirror].Position-state.Bodies[array].Position).Magnitude});
            file.WriteLine(string.Join(",",values.SelectInvariant())); file.Flush();
        }
        public MissionSummary Run(string folder,bool benchmarksPassed,Action<string> progress=null)
        {
            Directory.CreateDirectory(folder); string trajectory=Path.Combine(folder,"run_mission.csv"); bool partial=false;
            radiation.Evaluate(state); monitors.Check(state,ledger,stepper,radiation,climate,Materials);
            using(var output=new StreamWriter(trajectory))
            {
                output.WriteLine(Header()); WriteRow(output);
                long steps=(long)Math.Round((c.Years*Constants.JulianYear-state.Time)/c.Dt); int cadence=c.RecordEveryStep?1:Math.Max(1,(int)Math.Round(30*Units.Day/c.Dt)); int annual=Math.Max(1,(int)Math.Round((c.Years>=100?100:1)*Constants.JulianYear/c.Dt));
                for(long k=0;k<steps;k++)
                {
                    try { Advance(); }
                    catch(Exception error) { partial=true; monitors.Flags.Add(state.Time,"Stopped",error.Message); radiation.Evaluate(state); ledger.Check(state); monitors.Check(state,ledger,stepper,radiation,climate,Materials); WriteRow(output); break; }
                    bool checkpoint=false; foreach(double year in CheckpointYears) if(Math.Abs(state.Time-year*Constants.JulianYear)<c.Dt/2) checkpoint=true;
                    if(k%cadence==0 || k==steps-1 || checkpoint) WriteRow(output);
                    if(k%annual==0 || k==steps-1) progress?.Invoke("Mission year="+(state.Time/Constants.JulianYear).ToString("F3",CultureInfo.InvariantCulture)+" a/AU="+(monitors.A/Constants.AU).ToString("F6",CultureInfo.InvariantCulture)+" flags="+monitors.Flags.All.Length);
                    if(c.StopOnSafetyFlag && monitors.Flags.HasUnresolved) { monitors.Flags.Add(state.Time,"ConfiguredEarlyStop","Stopped on configured unresolved flag"); break; }
                }
            }
            double energyClosure=solarEnergy.Value==0?0:Math.Abs(solarEnergy.Value-solarReflected.Value-arrayHeat.Value-jetEnergy.Value-thrusterHeat.Value-unusedElectrical.Value)/solarEnergy.Value;
            bool complete=!partial && Math.Abs(state.Time-c.Years*Constants.JulianYear)<c.Dt/2;
            bool conservation=ledger.MaxEnergyError<=limits.ledgerMaximum.value && ledger.MaxMomentumError<=limits.ledgerMaximum.value && ledger.MaxAngularMomentumError<=limits.ledgerMaximum.value && stepper.MaxImpulseError<=limits.ledgerMaximum.value && energyClosure<=limits.ledgerMaximum.value;
            var conditions=new[]{
                new WinningCondition{id="target",description="Target orbit reached within 10000 years",passed=targetTime>=0 && targetTime<=10000*Constants.JulianYear,value="targetTime_s="+targetTime+"; final a/AU="+monitors.A/Constants.AU},
                new WinningCondition{id="safe",description="All placeholder planetary safety thresholds throughout run",passed=monitors.MinTemperature>=limits.temperatureMinimum.value && monitors.MaxTemperature<=limits.temperatureMaximum.value && monitors.AllMoonBound && !MissionFailureClassification.HasMeasuredFailure(monitors.Flags),value="temperature="+monitors.MinTemperature+".."+monitors.MaxTemperature+" K; MoonBound="+monitors.AllMoonBound},
                new WinningCondition{id="structure",description="Every modeled structural factor >=1.5",passed=monitors.MinSafetyFactor>=limits.minimumSafetyFactor.value,value="min factor="+monitors.MinSafetyFactor},
                new WinningCondition{id="benchmarks",description="Applicable M1/M2 benchmarks pass",passed=benchmarksPassed,value=benchmarksPassed?"recorded passing suite":"not passing/not read"},
                new WinningCondition{id="conservation",description="Mechanical, reaction and solar-energy budgets close",passed=conservation,value="P="+ledger.MaxMomentumError+"; L="+ledger.MaxAngularMomentumError+"; E="+ledger.MaxEnergyError+"; solar="+energyClosure},
                new WinningCondition{id="flags",description="No measured failure flags; A1+ assumptions remain declared",passed=!MissionFailureClassification.HasMeasuredFailure(monitors.Flags),value="flag count="+monitors.Flags.All.Length}};
            bool win=complete; foreach(var condition in conditions) win &= condition.passed;
            var summary=new MissionSummary{status=win?(limits.AnyPlaceholder?"CONDITIONAL WIN (A1+ unresolved; placeholder thresholds)":"CONDITIONAL WIN (A1+ unresolved)"):(limits.AnyPlaceholder?"FAIL (provisional: placeholder thresholds)":"FAIL"),scope="full N-body with finite-mass prescribed infrastructure and explicit solar supply",trajectoryFile=trajectory,seconds=state.Time,targetTimeSeconds=targetTime,finalA_AU=monitors.A/Constants.AU,finalE=monitors.E,completed=complete,partialStep=partial,placeholderThresholds=limits.AnyPlaceholder,temperatureMinimum=monitors.MinTemperature,temperatureMaximum=monitors.MaxTemperature,materialKg=Materials,jetEnergyJ=jetEnergy.Value,solarInputJ=solarEnergy.Value,sourceFraction=Math.Min(1,electricalUsed.Value/Math.Max(1,jetEnergy.Value/c.ThrusterEfficiency)),energyError=ledger.MaxEnergyError,momentumError=ledger.MaxMomentumError,angularError=ledger.MaxAngularMomentumError,impulseError=stepper.MaxImpulseError,reactionError=ledger.MaxReactionClosure,minimumSafetyFactor=monitors.MinSafetyFactor,maximumShade=monitors.MaxShade,conditions=conditions,flags=monitors.Flags.All,config=c,limitations="Giant focusing optics unresolved per user; simple EBM; illustrative material strengths; omitted coupled thermal/global optical structure; ballistic ejecta not propagated; predeployment/assembly not simulated; prescribed paths realized with discrete impulses; constant nominal solar luminosity; incoming IR treated using the EBM albedo approximation."};
            MissionConfigurationIO.Write(Path.Combine(folder,"mission_report.json"),summary);
            summary.finalEjectedKg=ejectedPrefix+stepper.Ejected; summary.postTargetEjectedKg=targetTime>=0?summary.finalEjectedKg-targetEjected:0; summary.materialHeadroomKg=c.MaterialCap-Materials; summary.branchStartSeconds=branchStart; summary.departureComplete=stepper.DepartureComplete;
            summary.maximumEarthShade=monitors.MaxEarthShade; summary.maximumMirrorShade=monitors.MaxMirrorShade; summary.maximumTideRatio=monitors.MaxTideRatio; summary.maximumFlux=monitors.MaxFlux; summary.minimumEarthPlumeClearance=monitors.EarthPlumeClearance; summary.minimumMoonPlumeClearance=monitors.MoonPlumeClearance;
            summary.maximumArrayJetPower=monitors.MaxArrayJetPower; summary.maximumMirrorJetPower=monitors.MaxMirrorJetPower; summary.maximumRockStress=monitors.MaxRockStress; summary.maximumArrayBendingStress=monitors.MaxArrayStress; summary.maximumMirrorBendingStress=monitors.MaxMirrorStress;
            MissionConfigurationIO.Write(Path.Combine(folder,"mission_report.json"),summary);
            File.WriteAllText(Path.Combine(folder,"ledger.txt"),ledger.Report()+"\nSolar energy sector relative closure="+energyClosure.ToString("R",CultureInfo.InvariantCulture));
            summary.sourceFraction=Math.Min(1,electricalUsed.Value/Math.Max(1,jetEnergy.Value/c.ThrusterEfficiency));
            var finalBodies=new FinalMissionBody[state.Bodies.Length];
            for(int k=0;k<finalBodies.Length;k++) { var b=state.Bodies[k]; finalBodies[k]=new FinalMissionBody{name=b.Name,mass=b.Mass,position=new[]{b.Position.X,b.Position.Y,b.Position.Z},velocity=new[]{b.Velocity.X,b.Velocity.Y,b.Velocity.Z}}; }
            MissionConfigurationIO.Write(Path.Combine(folder,"final_state.json"),new FinalMissionState{time=state.Time,bodies=finalBodies,partialStep=partial});
            if(lastValidCoastState!=null)
            {
                var safe=new FinalMissionBody[lastValidCoastState.Bodies.Length];
                for(int k=0;k<safe.Length;k++){var b=lastValidCoastState.Bodies[k]; safe[k]=new FinalMissionBody{name=b.Name,mass=b.Mass,position=new[]{b.Position.X,b.Position.Y,b.Position.Z},velocity=new[]{b.Velocity.X,b.Velocity.Y,b.Velocity.Z}};}
                MissionConfigurationIO.Write(Path.Combine(folder,"last_valid_coast_state.json"),new FinalMissionState{time=lastValidCoastState.Time,bodies=safe,partialStep=false});
            }
            MissionConfigurationIO.Write(Path.Combine(folder,"enablers.json"),new[]{new EnablerDeclaration{id="E1",description="Supplied raw material at consumer velocity; initial infrastructure plus later growth and all station-keeping propellant counted",bound=c.MaterialCap,units="kg"},new EnablerDeclaration{id="E2",description="Ideal thrusters/conversion/transmission/station keeping at the configured efficiency and exhaust speed",bound=c.ThrusterEfficiency,units="efficiency"},new EnablerDeclaration{id="E3",description="Tractor mass initially in place",bound=state.Bodies[tractor].Mass,units="kg"}});
            return summary;
        }
    }
}
