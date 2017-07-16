using System;
using System.Net;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.pspnet
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceNetResolver : HleModuleHost
    {
        public struct in_addr
        {
            public uint Address;

            public byte[] AddressAsBytes
            {
                set { Address = BitConverter.ToUInt32(value, 0); }
                get { return BitConverter.GetBytes(Address); }
            }

            public override string ToString()
            {
                var AddressAsBytes = this.AddressAsBytes;
                return String.Format("{0}.{1}.{2}.{3}", AddressAsBytes[0], AddressAsBytes[1], AddressAsBytes[2],
                    AddressAsBytes[3]);
            }
        }

        /// <summary>
        /// Inititalise the resolver library
        /// </summary>
        /// <returns>0 on sucess, less than 0 on error.</returns>
        [HlePspFunction(NID = 0xF3370E61, FirmwareVersion = 150)]
        public int sceNetResolverInit()
        {
            return 0;
        }

        /// <summary>
        /// Terminate the resolver library
        /// </summary>
        /// <returns>0 on sucess, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x6138194A, FirmwareVersion = 150)]
        public int sceNetResolverTerm()
        {
            return 0;
        }

        public class Resolver : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }

        HleUidPool<Resolver> Resolvers = new HleUidPool<Resolver>();

        /// <summary>
        /// Create a resolver object
        /// </summary>
        /// <param name="PointerToResolverId">Pointer to receive the resolver ID</param>
        /// <param name="Buffer">Temporary buffer</param>
        /// <param name="BufferLength">Length of the temporary buffer</param>
        /// <returns>0 on sucess, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x244172AF, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetResolverCreate(int*PointerToResolverId, void*Buffer, int BufferLength)
        {
            var Resolver = new Resolver();
            var ResolverId = Resolvers.Create(Resolver);
            *PointerToResolverId = ResolverId;
            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Delete a resolver
        /// </summary>
        /// <param name="ResolverId">The resolver to delete</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x94523E09, FirmwareVersion = 150)]
        public int sceNetResolverDelete(int ResolverId)
        {
            Resolvers.Remove(ResolverId);
            return 0;
        }

        /// <summary>
        /// Begin a name to address lookup
        /// </summary>
        /// <param name="ResolverId">Resolver id</param>
        /// <param name="HostName">Name to resolve</param>
        /// <param name="Address">Pointer to in_addr structure to receive the address</param>
        /// <param name="Timeout">Number of seconds before timeout</param>
        /// <param name="Retries">Number of retires</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x224C5F44, FirmwareVersion = 150)]
        public int sceNetResolverStartNtoA(int ResolverId, string HostName, in_addr* Address, uint Timeout, int Retries)
        {
            var Resolver = Resolvers.Get(ResolverId);
            var ResolvedAddress = Dns.GetHostEntry(HostName).AddressList[0];
            var Bytes = ResolvedAddress.GetAddressBytes();
            (*Address).AddressAsBytes = Bytes;
            //(*addr).Address = 
            //Resolver.Resolve();
            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Begin a address to name lookup
        /// </summary>
        /// <param name="rid">Resolver ID</param>
        /// <param name="addr">Pointer to the address to resolve</param>
        /// <param name="hostname">Buffer to receive the name</param>
        /// <param name="hostname_len">Length of the buffer</param>
        /// <param name="timeout">Number of seconds before timeout</param>
        /// <param name="retry">Number of retries</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x629E2FB7, FirmwareVersion = 150)]
        public int sceNetResolverStartAtoN(int rid, in_addr* addr, byte*hostname, int hostname_len, uint timeout,
            int retry)
        {
            throw(new NotImplementedException());
        }

        /// <summary>
        /// Stop a resolver operation
        /// </summary>
        /// <param name="rid">Resolver ID</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x808F6063, FirmwareVersion = 150)]
        public int sceNetResolverStop(int rid)
        {
            throw(new NotImplementedException());
        }
    }
}