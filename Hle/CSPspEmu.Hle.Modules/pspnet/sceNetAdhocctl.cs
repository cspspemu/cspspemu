using CSPspEmu.Hle.Managers;
using System;

namespace CSPspEmu.Hle.Modules.pspnet
{
    public unsafe partial class sceNetAdhocctl : HleModuleHost
    {
        [Inject] HleInterop HleInterop;


        /// <summary>
        /// Initialise the Adhoc control library
        /// </summary>
        /// <param name="stacksize">Stack size of the adhocctl thread. Set to 0x2000</param>
        /// <param name="priority">Priority of the adhocctl thread. Set to 0x30</param>
        /// <param name="product">Pass a filled in <see cref="productStruct"/></param>
        /// <returns>0 on success, &lt; 0 on error</returns>
        [HlePspFunction(NID = 0xE26F226E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlInit(int stacksize, int priority, productStruct* product)
        {
            CurrentState = State.Disconnected;
            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Terminate the Adhoc control library
        /// </summary>
        /// <returns>0 on success, &lt; on error.</returns>
        [HlePspFunction(NID = 0x9D689E13, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlTerm()
        {
            return 0;
        }

        State CurrentState;
        public string ConnectionName;

        /// <summary>
        /// Connect to the Adhoc control
        /// </summary>
        /// <param name="Name">The name of the connection (maximum 8 alphanumeric characters).</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x0AD043ED, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlConnect(string Name)
        {
            this.ConnectionName = Name;
            this.CurrentState = State.Connected;
            _notifyAdhocctlHandler(Event.Connected);
            return 0;
        }

        public enum Event : int
        {
            Error = 0,
            Connected = 1,
            Disconnected = 2,
            Scan = 3,
            Game = 4,
            Discover = 5,
            Wol = 6,
            WolInterrupted = 7,
        }

        public enum State : int
        {
            Disconnected = 0,
            Connected = 1,
            Scan = 2,
            Game = 3,
            Discover = 4,
            Wol = 5,
        }

        public enum Mode : int
        {
            Normal = 0,
            GameMode = 1,
            None = -1,
        }

        public const int NICK_NAME_LENGTH = 128;
        public const int GROUP_NAME_LENGTH = 8;
        public const int IBSS_NAME_LENGTH = 6;
        public const int ADHOC_ID_LENGTH = 9;
        public const int MAX_GAME_MODE_MACS = 16;

        private void _notifyAdhocctlHandler(Errors Error)
        {
            _notifyAdhocctlHandler(Event.Error, Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="Error"></param>
        private void _notifyAdhocctlHandler(Event @event, Errors Error = Errors.SUCCESS)
        {
            Console.Error.WriteLine("_notifyAdhocctlHandler:");
            foreach (var Handler in InjectContext.GetInstance<HleUidPoolManager>().List<AdhocctlHandler>())
            {
                Console.Error.WriteLine("_notifyAdhocctlHandler: {0:X8}: {1}: {2}, {3}", Handler.callback, @event,
                    Error, Handler.parameter);
                HleInterop.ExecuteFunctionLater(Handler.callback, (uint) @event, (uint) Error, Handler.parameter);
            }
        }

        /// <summary>
        /// Disconnect from the Adhoc control
        /// </summary>
        /// <returns>0 on success, &lt; 0 on error</returns>
        [HlePspFunction(NID = 0x34401D65, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlDisconnect()
        {
            this.CurrentState = State.Disconnected;
            _notifyAdhocctlHandler(Event.Disconnected);
            return 0;
        }

        /// <summary>
        /// Get the state of the Adhoc control
        /// </summary>
        /// <param name="State">Pointer to an integer to receive the status. Can continue when it becomes 1.</param>
        /// <returns>0 on success, &lt; 0 on error</returns>
        [HlePspFunction(NID = 0x75ECD386, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetState(out State State)
        {
            State = this.CurrentState;
            return 0;
        }

        /// <summary>
        /// Connect to the Adhoc control (as a host)
        /// </summary>
        /// <param name="Name">The name of the connection (maximum 8 alphanumeric characters).</param>
        /// <returns> on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xEC0635C1, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlCreate(string Name)
        {
            return 0;
        }

        /// <summary>
        /// Connect to the Adhoc control (as a client)
        /// </summary>
        /// <param name="scaninfo">A valid ::SceNetAdhocctlScanInfo struct that has been filled by sceNetAchocctlGetScanInfo</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x5E7F79C9, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlJoin(ref SceNetAdhocctlScanInfo scaninfo)
        {
            return 0;
        }

        /// <summary>
        /// Get the adhoc ID
        /// </summary>
        /// <param name="product">A pointer to a  <see cref="productStruct"/></param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x362CBE8F, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetAdhocId(ref productStruct product)
        {
            return 0;
        }

        /// <summary>
        /// Connect to the Adhoc control game mode (as a host)
        /// </summary>
        /// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
        /// <param name="unknown">Pass 1</param>
        /// <param name="num">The total number of players (including the host).</param>
        /// <param name="macs"> Pointer to a list of the participating mac addresses, host first, then clients.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="unknown2">Pass 0</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xA5C055CE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlCreateEnterGameMode(string name, int unknown, int num, string macs, uint timeout,
            int unknown2)
        {
            return 0;
        }

        /// <summary>
        /// Connect to the Adhoc control game mode (as a client)
        /// </summary>
        /// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
        /// <param name="hostmac">The mac address of the host.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="unknown">Pass 0</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x1FF89745, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlJoinEnterGameMode(string name, string hostmac, uint timeout, int unknown)
        {
            return 0;
        }

        /// <summary>
        /// Get game mode information
        /// </summary>
        /// <param name="gamemodeinfo">Pointer to store the info.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x5A014CE0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetGameModeInfo(SceNetAdhocctlGameModeInfo* gamemodeinfo)
        {
            return 0;
        }

        /// <summary>
        /// Exit game mode.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xCF8E084D, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlExitGameMode()
        {
            return 0;
        }

        /// <summary>
        /// Get a list of peers
        /// </summary>
        /// <param name="length">The length of the list.</param>
        /// <param name="buf">An allocated area of size length.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xE162CB14, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetPeerList(int* length, void* buf)
        {
            return 0;
        }

        /// <summary>
        /// Get peer information
        /// </summary>
        /// <param name="mac">The mac address of the peer.</param>
        /// <param name="size">Size of peerinfo.</param>
        /// <param name="peerinfo">Pointer to store the information.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x8DB83FDC, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetPeerInfo(string mac, int size, SceNetAdhocctlPeerInfo* peerinfo)
        {
            return 0;
        }

        /// <summary>
        /// Scan the adhoc channels
        /// </summary>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x08FFF7A0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlScan()
        {
            return 0;
        }

        /// <summary>
        /// Get the results of a scan
        /// </summary>
        /// <param name="length">The length of the list.</param>
        /// <param name="buf">An allocated area of size length.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x81AEE1BE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetScanInfo(int* length, void* buf)
        {
            return 0;
        }

        //typedef void (*sceNetAdhocctlHandler)(int flag, int error, void *unknown);

        delegate void sceNetAdhocctlHandler(int @event, int error, int unknown);

        public class AdhocctlHandler : IHleUidPoolClass
        {
            /// <summary>
            /// typedef void (*sceNetAdhocctlHandler)(int flag, int error, void *unknown);
            /// </summary>
            public uint callback;

            /// <summary>
            /// 
            /// </summary>
            public uint parameter;

            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
            }
        }

        public enum Errors : uint
        {
            ERROR_NET_BUFFER_TOO_SMALL = 0x80400706,

            ERROR_NET_RESOLVER_BAD_ID = 0x80410408,
            ERROR_NET_RESOLVER_ALREADY_STOPPED = 0x8041040a,
            ERROR_NET_RESOLVER_INVALID_HOST = 0x80410414,

            ERROR_NET_ADHOC_INVALID_SOCKET_ID = 0x80410701,
            ERROR_NET_ADHOC_INVALID_ADDR = 0x80410702,
            ERROR_NET_ADHOC_NO_DATA_AVAILABLE = 0x80410709,
            ERROR_NET_ADHOC_PORT_IN_USE = 0x8041070a,
            ERROR_NET_ADHOC_NOT_INITIALIZED = 0x80410712,
            ERROR_NET_ADHOC_ALREADY_INITIALIZED = 0x80410713,
            ERROR_NET_ADHOC_DISCONNECTED = 0x8041070c,
            ERROR_NET_ADHOC_TIMEOUT = 0x80410715,
            ERROR_NET_ADHOC_NO_ENTRY = 0x80410716,
            ERROR_NET_ADHOC_CONNECTION_REFUSED = 0x80410718,
            ERROR_NET_ADHOC_INVALID_MATCHING_ID = 0x80410807,
            ERROR_NET_ADHOC_MATCHING_ALREADY_INITIALIZED = 0x80410812,
            ERROR_NET_ADHOC_MATCHING_NOT_INITIALIZED = 0x80410813,

            ERROR_NET_ADHOCCTL_WLAN_SWITCH_OFF = 0x80410b03,
            ERROR_NET_ADHOCCTL_ALREADY_INITIALIZED = 0x80410b07,
            ERROR_NET_ADHOCCTL_NOT_INITIALIZED = 0x80410b08,
            ERROR_NET_ADHOCCTL_DISCONNECTED = 0x80410b09,
            ERROR_NET_ADHOCCTL_BUSY = 0x80410b10,
            ERROR_NET_ADHOCCTL_TOO_MANY_HANDLERS = 0x80410b12,

            SUCCESS = 0,
        };

        /// <summary>
        /// Register an adhoc event handler
        /// </summary>
        /// <param name="callback">The event handler.</param>
        /// <param name="parameter">Pass NULL</param>
        /// <returns>Handler ID on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x20B317A0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public AdhocctlHandler sceNetAdhocctlAddHandler( /*sceNetAdhocctlHandler*/ uint callback, uint parameter)
        {
            return new AdhocctlHandler()
            {
                callback = callback,
                parameter = parameter,
            };
        }

        /// <summary>
        /// Delete an adhoc event handler
        /// </summary>
        /// <param name="id">he handler ID as returned by <see cref="sceNetAdhocctlAddHandler"/>.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x6402490B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlDelHandler(AdhocctlHandler handler)
        {
            handler.RemoveUid(InjectContext);
            return 0;
        }

        /// <summary>
        /// Get nickname from a mac address
        /// </summary>
        /// <param name="Mac">The mac address.</param>
        /// <param name="Nickname">Pointer to a char buffer where the nickname will be stored.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x8916C003, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetNameByAddr(byte* Mac, string Nickname)
        {
            return 0;
        }

        /// <summary>
        /// Get mac address from nickname
        /// </summary>
        /// <param name="NickName">The nickname.</param>
        /// <param name="length">The length of the list.</param>
        /// <param name="buf">An allocated area of size length.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x99560ABE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetAddrByName(string NickName, int* length, void* buf)
        {
            return 0;
        }

        /// <summary>
        /// Get Adhocctl parameter
        /// </summary>
        /// <param name="Params">Pointer to a <see cref="SceNetAdhocctlParams"/></param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xDED9D28E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocctlGetParameter(SceNetAdhocctlParams* Params)
        {
            return 0;
        }

        /// <summary>
        /// Product structure
        /// </summary>
        public struct productStruct
        {
            /// <summary>
            /// Unknown, set to 0
            /// </summary>
            public int unknown;

            /// <summary>
            /// The product ID string
            /// </summary>
            public fixed byte product[9];
        }

        /// <summary>
        /// Peer info structure
        /// </summary>
        public struct SceNetAdhocctlPeerInfo
        {
            /// <summary>
            /// 
            /// </summary>
            //public SceNetAdhocctlPeerInfo *next;
            public uint nextPointer;

            /// <summary>
            /// Nickname
            /// </summary>
            public fixed byte nickname[128];

            /// <summary>
            /// Mac address
            /// </summary>
            public fixed byte mac[6];

            /// <summary>
            /// Unknown
            /// </summary>
            public fixed byte unknown[6];

            /// <summary>
            /// Time stamp
            /// </summary>
            public uint timestamp;
        }

        /// <summary>
        /// Scan info structure
        /// </summary>
        public struct SceNetAdhocctlScanInfo
        {
            /// <summary>
            /// 
            /// </summary>
            //SceNetAdhocctlScanInfo *next;
            public uint nextPointer;

            /// <summary>
            /// Channel number
            /// </summary>
            public int channel;

            /// <summary>
            /// Name of the connection (alphanumeric characters only)
            /// </summary>
            public fixed byte name[8];

            /// <summary>
            /// The BSSID
            /// </summary>
            public fixed byte bssid[6];

            /// <summary>
            /// Unknown
            /// </summary>
            public fixed byte unknown[2];

            /// <summary>
            /// Unknown
            /// </summary>
            public int unknown2;
        }

        public struct SceNetAdhocctlGameModeInfo
        {
            /// <summary>
            /// Number of peers (including self)
            /// </summary>
            public int count;

            /// <summary>
            /// MAC addresses of peers (including self)
            /// </summary>
            //byte macs[16][6];
            public fixed byte macs[16 * 6];
        }

        /// <summary>
        /// Params structure
        /// </summary>
        public struct SceNetAdhocctlParams
        {
            /// <summary>
            /// Channel number
            /// </summary>
            public int channel;

            /// <summary>
            /// Name of the connection
            /// </summary>
            public fixed byte name[8];

            /// <summary>
            /// The BSSID
            /// </summary>
            public fixed byte bssid[6];

            /// <summary>
            /// Nickname
            /// </summary>
            public fixed byte nickname[128];
        }
    }
}