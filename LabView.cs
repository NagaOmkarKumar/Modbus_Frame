using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wipro
{
    public class Labview
    {
        public string MotorName { get; set; }
        public string RequestNumber { get; set; }
        public double Temparature { get; set; }
        public double TargetCycles { get; set; }
        public double NoofStages { get; set; }
        public double FWDFlowRate { get; set; }
        public double REVFlowRate { get; set; }
        public double ClosedLength { get; set; }
        public double OpenLength { get; set; }
        public double CoolingMotor { get; set; }
        public double HundredHP { get; set; }
        public double CoolingOccupation { get; set; }
        public double Pressure1 { get; set; }
        public double Pressure2 { get; set; }
        public double Pressure3 { get; set; }
        public double Pressure4 { get; set; }
        public string WarningMessages { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

    }
    public class LimitSwitches
    {
        public bool FillterLS { get; set; }
        public bool CoolerLS { get; set; }
        public bool TanktoSideLoadLS { get; set; }
        public bool TanktoLoadLS { get; set; }
        public bool TanktoCylinderLS { get; set; }
        public bool OilLevelLow { get; set; }
        public bool OilTemperatureT1 { get; set; }
        public bool OilTemperatureT2 { get; set; }
        public bool FillterationMotorON { get; set; }
        public bool CoolerMotorON { get; set; }

    }
    public class MotorValveStatus
    {
        public bool CylinderMotorON { get; set; }
        public bool LoadingMotorON { get; set; }
        public bool SideLoadMotorON { get; set; }
        public bool Solenoid1 { get; set; }
        public bool Solenoid2 { get; set; }
        public bool Solenoid3 { get; set; }
        public bool Solenoid4 { get; set; }
        public bool Solenoid5 { get; set; }
        public bool Solenoid6 { get; set; }
    }
}
