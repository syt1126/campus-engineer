using System;
using CampusEngineer;
public static class SimulationChecks
{
    static int assertions;
    static void Check(bool condition,string label){assertions++;if(!condition)throw new Exception(label);}
    public static void Main()
    {
        var ideal=new ControlRules();var a=CampusSimulation.Run(ideal,0);
        Check(a.Passed,"Reference solution must pass");
        Check(a.ticks.Count==40,"40 simulation minutes");
        Check(Math.Abs(a.kwh-0.005833333333333334)<1e-10,"Energy unit conversion");
        Check(Math.Abs(a.Savings-.75)<1e-10,"Model savings");
        Check(CampusSimulation.Run(new ControlRules{forceOff=true},0).errors>0,"All-off must not pass");
        Check(CampusSimulation.Run(new ControlRules{forceOn=true},0).errors>0,"All-on must not pass");
        Check(CampusSimulation.Run(new ControlRules{requireOccupied=false},0).emptyActiveFrames>0,"Missing occupancy must be found");
        Check(CampusSimulation.Run(ideal,2).Passed,"Independent boundary challenge");
        Check(CampusSimulation.Run(new ControlRules{fanThreshold=29},2).errors>0,"Inclusive temperature boundary");
        Check(CampusSimulation.Run(new ControlRules{lightThreshold=301},2).errors>0,"Exclusive lux boundary");
        Check(CampusSimulation.Run(new ControlRules{hysteresis=true},1).Passed,"Hysteresis scenario");
        Check(!CampusSimulation.Run(ideal,1).Passed,"Single threshold cannot fake hysteresis");
        Check(CampusSimulation.Run(ideal,0).kwh==a.kwh,"Deterministic replay");
        Check(a.errors==0 && a.emptyActiveFrames==0 && a.servicedFrames==20,"Reference record consistency");
        bool threw=false;try{CampusSimulation.Run(new ControlRules{fanThreshold=float.NaN},0);}catch(ArgumentException){threw=true;}
        Check(threw,"Reject invalid thresholds");
        Console.WriteLine("PASS "+assertions+" simulation invariants");
    }
}
