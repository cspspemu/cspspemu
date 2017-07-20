using System;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        //string LightArrayOperation(string type, string code, int step = 1) { return ArrayOperation(type, 0, 3, code, step); }
        //string LightArrayOperationStep3(string type, string code) { return LightArrayOperation(type, code, 3); }

        [GpuInstructionAttribute(GpuOpCodes.LTE)]
        public void OP_LTE() => GpuState->LightingState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.ALC)]
        public void OP_ALC() => GpuState->LightingState.AmbientLightColor.SetRgb(Params24);

        [GpuInstructionAttribute(GpuOpCodes.ALA)]
        public void OP_ALA() => GpuState->LightingState.AmbientLightColor.SetA(Params24);

        [GpuInstructionAttribute(GpuOpCodes.LMODE)]
        public void OP_LMODE() => GpuState->LightingState.LightModel = (LightModelEnum) Param8(0);

        LightStateStruct* GetLigth(int index) => &((&GpuState->LightingState.Light0)[index]);

        private void _OP_LTE(int index) => GetLigth(index)->Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.LTE0)]
        public void OP_LTE0() => _OP_LTE(0);

        [GpuInstructionAttribute(GpuOpCodes.LTE1)]
        public void OP_LTE1() => _OP_LTE(1);

        [GpuInstructionAttribute(GpuOpCodes.LTE2)]
        public void OP_LTE2() => _OP_LTE(2);

        [GpuInstructionAttribute(GpuOpCodes.LTE3)]
        public void OP_LTE3() => _OP_LTE(3);

        private void _OP_LXP(int index) => GetLigth(index)->Position.X = Float1;
        private void _OP_LYP(int index) => GetLigth(index)->Position.Y = Float1;
        private void _OP_LZP(int index) => GetLigth(index)->Position.Z = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LXP0)]
        public void OP_LXP0() => _OP_LXP(0);

        [GpuInstructionAttribute(GpuOpCodes.LXP1)]
        public void OP_LXP1() => _OP_LXP(1);

        [GpuInstructionAttribute(GpuOpCodes.LXP2)]
        public void OP_LXP2() => _OP_LXP(2);

        [GpuInstructionAttribute(GpuOpCodes.LXP3)]
        public void OP_LXP3() => _OP_LXP(3);

        [GpuInstructionAttribute(GpuOpCodes.LYP0)]
        public void OP_LYP0() => _OP_LYP(0);

        [GpuInstructionAttribute(GpuOpCodes.LYP1)]
        public void OP_LYP1() => _OP_LYP(1);

        [GpuInstructionAttribute(GpuOpCodes.LYP2)]
        public void OP_LYP2() => _OP_LYP(2);

        [GpuInstructionAttribute(GpuOpCodes.LYP3)]
        public void OP_LYP3() => _OP_LYP(3);

        [GpuInstructionAttribute(GpuOpCodes.LZP0)]
        public void OP_LZP0() => _OP_LZP(0);

        [GpuInstructionAttribute(GpuOpCodes.LZP1)]
        public void OP_LZP1() => _OP_LZP(1);

        [GpuInstructionAttribute(GpuOpCodes.LZP2)]
        public void OP_LZP2() => _OP_LZP(2);

        [GpuInstructionAttribute(GpuOpCodes.LZP3)]
        public void OP_LZP3() => _OP_LZP(3);

        private void _OP_LT(int index)
        {
            GetLigth(index)->Kind = (LightModelEnum) Param8(0);
            GetLigth(index)->Type = (LightTypeEnum) Param8(8);
            switch (GetLigth(index)->Type)
            {
                case LightTypeEnum.Directional:
                    GetLigth(index)->Position.W = 0;
                    break;
                case LightTypeEnum.PointLight:
                    GetLigth(index)->Position.W = 1;
                    GetLigth(index)->SpotCutoff = 180;
                    break;
                case LightTypeEnum.SpotLight:
                    GetLigth(index)->Position.W = 1;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [GpuInstructionAttribute(GpuOpCodes.LT0)]
        public void OP_LT0() => _OP_LT(0);

        [GpuInstructionAttribute(GpuOpCodes.LT1)]
        public void OP_LT1() => _OP_LT(1);

        [GpuInstructionAttribute(GpuOpCodes.LT2)]
        public void OP_LT2() => _OP_LT(2);

        [GpuInstructionAttribute(GpuOpCodes.LT3)]
        public void OP_LT3() => _OP_LT(3);

        private void _OP_LCA(int index) => GetLigth(index)->Attenuation.Constant = Float1;
        private void _OP_LLA(int index) => GetLigth(index)->Attenuation.Linear = Float1;
        private void _OP_LQA(int index) => GetLigth(index)->Attenuation.Quadratic = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LCA0)]
        public void OP_LCA0() => _OP_LCA(0);

        [GpuInstructionAttribute(GpuOpCodes.LCA1)]
        public void OP_LCA1() => _OP_LCA(1);

        [GpuInstructionAttribute(GpuOpCodes.LCA2)]
        public void OP_LCA2() => _OP_LCA(2);

        [GpuInstructionAttribute(GpuOpCodes.LCA3)]
        public void OP_LCA3() => _OP_LCA(3);

        [GpuInstructionAttribute(GpuOpCodes.LLA0)]
        public void OP_LLA0() => _OP_LLA(0);

        [GpuInstructionAttribute(GpuOpCodes.LLA1)]
        public void OP_LLA1() => _OP_LLA(1);

        [GpuInstructionAttribute(GpuOpCodes.LLA2)]
        public void OP_LLA2() => _OP_LLA(2);

        [GpuInstructionAttribute(GpuOpCodes.LLA3)]
        public void OP_LLA3() => _OP_LLA(3);

        [GpuInstructionAttribute(GpuOpCodes.LQA0)]
        public void OP_LQA0() => _OP_LQA(0);

        [GpuInstructionAttribute(GpuOpCodes.LQA1)]
        public void OP_LQA1() => _OP_LQA(1);

        [GpuInstructionAttribute(GpuOpCodes.LQA2)]
        public void OP_LQA2() => _OP_LQA(2);

        [GpuInstructionAttribute(GpuOpCodes.LQA3)]
        public void OP_LQA3() => _OP_LQA(3);

        private void _OP_LXD(int index) => GetLigth(index)->SpotDirection.X = Float1;
        private void _OP_LYD(int index) => GetLigth(index)->SpotDirection.Y = Float1;
        private void _OP_LZD(int index) => GetLigth(index)->SpotDirection.Z = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LXD0)]
        public void OP_LXD0() => _OP_LXD(0);

        [GpuInstructionAttribute(GpuOpCodes.LXD1)]
        public void OP_LXD1() => _OP_LXD(1);

        [GpuInstructionAttribute(GpuOpCodes.LXD2)]
        public void OP_LXD2() => _OP_LXD(2);

        [GpuInstructionAttribute(GpuOpCodes.LXD3)]
        public void OP_LXD3() => _OP_LXD(3);

        [GpuInstructionAttribute(GpuOpCodes.LYD0)]
        public void OP_LYD0() => _OP_LYD(0);

        [GpuInstructionAttribute(GpuOpCodes.LYD1)]
        public void OP_LYD1() => _OP_LYD(1);

        [GpuInstructionAttribute(GpuOpCodes.LYD2)]
        public void OP_LYD2() => _OP_LYD(2);

        [GpuInstructionAttribute(GpuOpCodes.LYD3)]
        public void OP_LYD3() => _OP_LYD(3);

        [GpuInstructionAttribute(GpuOpCodes.LZD0)]
        public void OP_LZD0() => _OP_LZD(0);

        [GpuInstructionAttribute(GpuOpCodes.LZD1)]
        public void OP_LZD1() => _OP_LZD(1);

        [GpuInstructionAttribute(GpuOpCodes.LZD2)]
        public void OP_LZD2() => _OP_LZD(2);

        [GpuInstructionAttribute(GpuOpCodes.LZD3)]
        public void OP_LZD3() => _OP_LZD(3);

        private void _OP_SPOTEXP(int index) => GetLigth(index)->SpotExponent = Float1;
        private void _OP_SPOTCUT(int index) => GetLigth(index)->SpotCutoff = Float1;

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP0)]
        public void OP_SPOTEXP0() => _OP_SPOTEXP(0);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP1)]
        public void OP_SPOTEXP1() => _OP_SPOTEXP(1);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP2)]
        public void OP_SPOTEXP2() => _OP_SPOTEXP(2);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP3)]
        public void OP_SPOTEXP3() => _OP_SPOTEXP(3);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT0)]
        public void OP_SPOTCUT0() => _OP_SPOTCUT(0);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT1)]
        public void OP_SPOTCUT1() => _OP_SPOTCUT(1);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT2)]
        public void OP_SPOTCUT2() => _OP_SPOTCUT(2);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT3)]
        public void OP_SPOTCUT3() => _OP_SPOTCUT(3);

        private void _OP_ALC(int index) => GetLigth(index)->AmbientColor.SetRGB_A1(Params24);
        private void _OP_DLC(int index) => GetLigth(index)->DiffuseColor.SetRGB_A1(Params24);
        private void _OP_SLC(int index) => GetLigth(index)->SpecularColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.ALC0)]
        public void OP_ALC0() => _OP_ALC(0);

        [GpuInstructionAttribute(GpuOpCodes.ALC1)]
        public void OP_ALC1() => _OP_ALC(1);

        [GpuInstructionAttribute(GpuOpCodes.ALC2)]
        public void OP_ALC2() => _OP_ALC(2);

        [GpuInstructionAttribute(GpuOpCodes.ALC3)]
        public void OP_ALC3() => _OP_ALC(3);

        [GpuInstructionAttribute(GpuOpCodes.DLC0)]
        public void OP_DLC0() => _OP_DLC(0);

        [GpuInstructionAttribute(GpuOpCodes.DLC1)]
        public void OP_DLC1() => _OP_DLC(1);

        [GpuInstructionAttribute(GpuOpCodes.DLC2)]
        public void OP_DLC2() => _OP_DLC(2);

        [GpuInstructionAttribute(GpuOpCodes.DLC3)]
        public void OP_DLC3() => _OP_DLC(3);

        [GpuInstructionAttribute(GpuOpCodes.SLC0)]
        public void OP_SLC0() => _OP_SLC(0);

        [GpuInstructionAttribute(GpuOpCodes.SLC1)]
        public void OP_SLC1() => _OP_SLC(1);

        [GpuInstructionAttribute(GpuOpCodes.SLC2)]
        public void OP_SLC2() => _OP_SLC(2);

        [GpuInstructionAttribute(GpuOpCodes.SLC3)]
        public void OP_SLC3() => _OP_SLC(3);

        [GpuInstructionAttribute(GpuOpCodes.SPOW)]
        public void OP_SPOW() => GpuState->LightingState.SpecularPower = Float1;
    }
}