using System;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe partial class Kirk
    {
        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_GEN_KEYS
        /// 
        /// Command: 12, 0xC
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd12(byte* Out, int outsize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT
        /// 
        /// Command: 13, 0xD
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd13(byte* Out, int outsize, byte* In, int insize)
        {
            //var ecdsa = ECDsa.Create();
            //ecdsa.
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_SIGN
        /// 
        /// Command: 16, 0x10
        /// </summary>
        /// <param name="Out"></param>
        /// <param name="outsize"></param>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd16(byte* Out, int outsize, byte* In, int insize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PSP_KIRK_CMD_ECDSA_VERIFY
        /// 
        /// Command: 17, 0x11
        /// </summary>
        /// <param name="In"></param>
        /// <param name="insize"></param>
        /// <returns></returns>
        public void ExecuteKirkCmd17(byte* In, int insize)
        {
            throw new NotImplementedException();
        }
    }
}