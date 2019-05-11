namespace CSPspEmu.Hle
{
    public enum SceKernelErrors : int
    {
        /*
         * PSP Errors:
         * Represented by a 32-bit value with the following scheme:
         *
         *  31  30  29  28  27        16  15        0
         * | 1 | 0 | 0 | 0 | X | ... | X | E |... | E |
         *
         * Bits 31 and 30: Can only be 1 or 0.
         *      -> If both are 0), there's no error (0x0==SUCCESS).
         *      -> If 31 is 1 but 30 is 0), there's an error (0x80000000).
         *      -> If both bits are 1), a critical error stops the PSP (0xC0000000).
         *
         * Bits 29 and 28: Unknown. Never change.
         *
         * Bits 27 to 16 (X): Represent the system area associated with the error.
         *      -> 0x000 - Null (can be used anywhere).
         *      -> 0x001 - Errno (PSP's implementation of errno.h).
         *      -> 0x002 - Kernel.
         *      -> 0x011 - Utility.
         *      -> 0x021 - UMD.
         *      -> 0x022 - MemStick.
         *      -> 0x026 - Audio.
         *      -> 0x02b - Power.
         *      -> 0x041 - Wlan.
         *      -> 0x042 - SAS.
         *      -> 0x043 - HTTP(0x0431)/HTTPS/SSL(0x0435).
         *      -> 0x044 - WAVE.
         *      -> 0x046 - Font.
         *      -> 0x061 - MPEG(0x0618)/PSMF(0x0615)/PSMF Player(0x0616).
         *      -> 0x062 - AVC.
         *      -> 0x063 - ATRAC.
         *      -> 0x07f - Codec.
         *
         * Bits 15 to 0 (E): Represent the error code itself (different for each area).
         *      -> E.g.: 0x80110001 - Error -> Utility -> Some unknown error.
         */

        ERROR_OK = unchecked((int) 0x00000000),

        ERROR_ERROR = unchecked((int) 0x80020001),
        ERROR_NOTIMP = unchecked((int) 0x80020002),

        ERROR_ALREADY = unchecked((int) 0x80000020),
        ERROR_BUSY = unchecked((int) 0x80000021),
        ERROR_OUT_OF_MEMORY = unchecked((int) 0x80000022),

        ERROR_INVALID_ID = unchecked((int) 0x80000100),
        ERROR_INVALID_NAME = unchecked((int) 0x80000101),
        ERROR_INVALID_INDEX = unchecked((int) 0x80000102),
        ERROR_INVALID_POINTER = unchecked((int) 0x80000103),
        ERROR_INVALID_SIZE = unchecked((int) 0x80000104),
        ERROR_INVALID_FLAG = unchecked((int) 0x80000105),
        ERROR_INVALID_COMMAND = unchecked((int) 0x80000106),
        ERROR_INVALID_MODE = unchecked((int) 0x80000107),
        ERROR_INVALID_FORMAT = unchecked((int) 0x80000108),
        ERROR_INVALID_VALUE = unchecked((int) 0x800001FE),
        ERROR_INVALID_ARGUMENT = unchecked((int) 0x800001FF),

        ERROR_BAD_FILE = unchecked((int) 0x80000209),
        ERROR_ACCESS_ERROR = unchecked((int) 0x8000020D),

        ERROR_ERRNO_OPERATION_NOT_PERMITTED = unchecked((int) 0x80010001),
        ERROR_ERRNO_FILE_NOT_FOUND = unchecked((int) 0x80010002),
        ERROR_ERRNO_FILE_OPEN_ERROR = unchecked((int) 0x80010003),
        ERROR_ERRNO_IO_ERROR = unchecked((int) 0x80010005),
        ERROR_ERRNO_ARG_LIST_TOO_LONG = unchecked((int) 0x80010007),
        ERROR_ERRNO_INVALID_FILE_DESCRIPTOR = unchecked((int) 0x80010009),
        ERROR_ERRNO_RESOURCE_UNAVAILABLE = unchecked((int) 0x8001000B),
        ERROR_ERRNO_NO_MEMORY = unchecked((int) 0x8001000C),
        ERROR_ERRNO_NO_PERM = unchecked((int) 0x8001000D),
        ERROR_ERRNO_FILE_INVALID_ADDR = unchecked((int) 0x8001000E),
        ERROR_ERRNO_DEVICE_BUSY = unchecked((int) 0x80010010),
        ERROR_ERRNO_FILE_ALREADY_EXISTS = unchecked((int) 0x80010011),
        ERROR_ERRNO_CROSS_DEV_LINK = unchecked((int) 0x80010012),
        ERROR_ERRNO_DEVICE_NOT_FOUND = unchecked((int) 0x80010013),
        ERROR_ERRNO_NOT_A_DIRECTORY = unchecked((int) 0x80010014),
        ERROR_ERRNO_IS_DIRECTORY = unchecked((int) 0x80010015),
        ERROR_ERRNO_INVALID_ARGUMENT = unchecked((int) 0x80010016),
        ERROR_ERRNO_TOO_MANY_OPEN_SYSTEM_FILES = unchecked((int) 0x80010018),
        ERROR_ERRNO_FILE_IS_TOO_BIG = unchecked((int) 0x8001001B),
        ERROR_ERRNO_DEVICE_NO_FREE_SPACE = unchecked((int) 0x8001001C),
        ERROR_ERRNO_READ_ONLY = unchecked((int) 0x8001001E),
        ERROR_ERRNO_CLOSED = unchecked((int) 0x80010020),
        ERROR_ERRNO_FILE_PATH_TOO_LONG = unchecked((int) 0x80010024),
        ERROR_ERRNO_FILE_PROTOCOL = unchecked((int) 0x80010047),
        ERROR_ERRNO_DIRECTORY_IS_NOT_EMPTY = unchecked((int) 0x8001005A),
        ERROR_ERRNO_TOO_MANY_SYMBOLIC_LINKS = unchecked((int) 0x8001005C),
        ERROR_ERRNO_FILE_ADDR_IN_USE = unchecked((int) 0x80010062),
        ERROR_ERRNO_CONNECTION_ABORTED = unchecked((int) 0x80010067),
        ERROR_ERRNO_CONNECTION_RESET = unchecked((int) 0x80010068),
        ERROR_ERRNO_NO_FREE_BUF_SPACE = unchecked((int) 0x80010069),
        ERROR_ERRNO_FILE_TIMEOUT = unchecked((int) 0x8001006E),
        ERROR_ERRNO_IN_PROGRESS = unchecked((int) 0x80010077),
        ERROR_ERRNO_ALREADY = unchecked((int) 0x80010078),
        ERROR_ERRNO_NO_MEDIA = unchecked((int) 0x8001007B),
        ERROR_ERRNO_INVALID_MEDIUM = unchecked((int) 0x8001007C),
        ERROR_ERRNO_ADDRESS_NOT_AVAILABLE = unchecked((int) 0x8001007D),
        ERROR_ERRNO_IS_ALREADY_CONNECTED = unchecked((int) 0x8001007F),
        ERROR_ERRNO_NOT_CONNECTED = unchecked((int) 0x80010080),
        ERROR_ERRNO_FILE_QUOTA_EXCEEDED = unchecked((int) 0x80010084),
        ERROR_ERRNO_FUNCTION_NOT_SUPPORTED = unchecked((int) 0x8001B000),
        ERROR_ERRNO_ADDR_OUT_OF_MAIN_MEM = unchecked((int) 0x8001B001),
        ERROR_ERRNO_INVALID_UNIT_NUM = unchecked((int) 0x8001B002),
        ERROR_ERRNO_INVALID_FILE_SIZE = unchecked((int) 0x8001B003),
        ERROR_ERRNO_INVALID_FLAG = unchecked((int) 0x8001B004),

        ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT = unchecked((int) 0x80020064),
        ERROR_KERNEL_INTERRUPTS_ALREADY_DISABLED = unchecked((int) 0x80020066),
        ERROR_KERNEL_UNKNOWN_UID = unchecked((int) 0x800200cb),
        ERROR_KERNEL_UNMATCH_TYPE_UID = unchecked((int) 0x800200cc),
        ERROR_KERNEL_NOT_EXIST_ID = unchecked((int) 0x800200cd),
        ERROR_KERNEL_NOT_FOUND_FUNCTION_UID = unchecked((int) 0x800200ce),
        ERROR_KERNEL_ALREADY_HOLDER_UID = unchecked((int) 0x800200cf),
        ERROR_KERNEL_NOT_HOLDER_UID = unchecked((int) 0x800200d0),
        ERROR_KERNEL_ILLEGAL_PERMISSION = unchecked((int) 0x800200d1),
        ERROR_KERNEL_ILLEGAL_ARGUMENT = unchecked((int) 0x800200d2),
        ERROR_KERNEL_ILLEGAL_ADDR = unchecked((int) 0x800200d3),
        ERROR_KERNEL_MEMORY_AREA_OUT_OF_RANGE = unchecked((int) 0x800200d4),
        ERROR_KERNEL_MEMORY_AREA_IS_OVERLAP = unchecked((int) 0x800200d5),
        ERROR_KERNEL_ILLEGAL_PARTITION_ID = unchecked((int) 0x800200d6),
        ERROR_KERNEL_PARTITION_IN_USE = unchecked((int) 0x800200d7),
        ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE = unchecked((int) 0x800200d8),
        ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK = unchecked((int) 0x800200d9),
        ERROR_KERNEL_INHIBITED_RESIZE_MEMBLOCK = unchecked((int) 0x800200da),
        ERROR_KERNEL_FAILED_RESIZE_MEMBLOCK = unchecked((int) 0x800200db),
        ERROR_KERNEL_FAILED_ALLOC_HEAPBLOCK = unchecked((int) 0x800200dc),
        ERROR_KERNEL_FAILED_ALLOC_HEAP = unchecked((int) 0x800200dd),
        ERROR_KERNEL_ILLEGAL_CHUNK_ID = unchecked((int) 0x800200de),
        ERROR_KERNEL_CANNOT_FIND_CHUNK_NAME = unchecked((int) 0x800200df),
        ERROR_KERNEL_NO_FREE_CHUNK = unchecked((int) 0x800200e0),
        ERROR_KERNEL_MEMBLOCK_FRAGMENTED = unchecked((int) 0x800200e1),
        ERROR_KERNEL_MEMBLOCK_CANNOT_JOINT = unchecked((int) 0x800200e2),
        ERROR_KERNEL_MEMBLOCK_CANNOT_SEPARATE = unchecked((int) 0x800200e3),
        ERROR_KERNEL_ILLEGAL_ALIGNMENT_SIZE = unchecked((int) 0x800200e4),
        ERROR_KERNEL_ILLEGAL_DEVKIT_VER = unchecked((int) 0x800200e5),

        ERROR_KERNEL_MODULE_LINK_ERROR = unchecked((int) 0x8002012c),
        ERROR_KERNEL_ILLEGAL_OBJECT_FORMAT = unchecked((int) 0x8002012d),
        ERROR_KERNEL_UNKNOWN_MODULE = unchecked((int) 0x8002012e),
        ERROR_KERNEL_UNKNOWN_MODULE_FILE = unchecked((int) 0x8002012f),
        ERROR_KERNEL_FILE_READ_ERROR = unchecked((int) 0x80020130),
        ERROR_KERNEL_MEMORY_IN_USE = unchecked((int) 0x80020131),
        ERROR_KERNEL_PARTITION_MISMATCH = unchecked((int) 0x80020132),
        ERROR_KERNEL_MODULE_ALREADY_STARTED = unchecked((int) 0x80020133),
        ERROR_KERNEL_MODULE_NOT_STARTED = unchecked((int) 0x80020134),
        ERROR_KERNEL_MODULE_ALREADY_STOPPED = unchecked((int) 0x80020135),
        ERROR_KERNEL_MODULE_CANNOT_STOP = unchecked((int) 0x80020136),
        ERROR_KERNEL_MODULE_NOT_STOPPED = unchecked((int) 0x80020137),
        ERROR_KERNEL_MODULE_CANNOT_REMOVE = unchecked((int) 0x80020138),
        ERROR_KERNEL_EXCLUSIVE_LOAD = unchecked((int) 0x80020139),
        ERROR_KERNEL_LIBRARY_IS_NOT_LINKED = unchecked((int) 0x8002013a),
        ERROR_KERNEL_LIBRARY_ALREADY_EXISTS = unchecked((int) 0x8002013b),
        ERROR_KERNEL_LIBRARY_NOT_FOUND = unchecked((int) 0x8002013c),
        ERROR_KERNEL_ILLEGAL_LIBRARY_HEADER = unchecked((int) 0x8002013d),
        ERROR_KERNEL_LIBRARY_IN_USE = unchecked((int) 0x8002013e),
        ERROR_KERNEL_MODULE_ALREADY_STOPPING = unchecked((int) 0x8002013f),
        ERROR_KERNEL_ILLEGAL_OFFSET_VALUE = unchecked((int) 0x80020140),
        ERROR_KERNEL_ILLEGAL_POSITION_CODE = unchecked((int) 0x80020141),
        ERROR_KERNEL_ILLEGAL_ACCESS_CODE = unchecked((int) 0x80020142),
        ERROR_KERNEL_MODULE_MANAGER_BUSY = unchecked((int) 0x80020143),
        ERROR_KERNEL_ILLEGAL_FLAG = unchecked((int) 0x80020144),
        ERROR_KERNEL_CANNOT_GET_MODULE_LIST = unchecked((int) 0x80020145),
        ERROR_KERNEL_PROHIBIT_LOADMODULE_DEVICE = unchecked((int) 0x80020146),
        ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE = unchecked((int) 0x80020147),
        ERROR_KERNEL_UNSUPPORTED_PRX_TYPE = unchecked((int) 0x80020148),
        ERROR_KERNEL_ILLEGAL_PERMISSION_CALL = unchecked((int) 0x80020149),
        ERROR_KERNEL_CANNOT_GET_MODULE_INFO = unchecked((int) 0x8002014a),
        ERROR_KERNEL_ILLEGAL_LOADEXEC_BUFFER = unchecked((int) 0x8002014b),
        ERROR_KERNEL_ILLEGAL_LOADEXEC_FILENAME = unchecked((int) 0x8002014c),
        ERROR_KERNEL_NO_EXIT_CALLBACK = unchecked((int) 0x8002014d),
        ERROR_KERNEL_MEDIA_CHANGED = unchecked((int) 0x8002014e),
        ERROR_KERNEL_CANNOT_USE_BETA_VER_MODULE = unchecked((int) 0x8002014f),

        ERROR_KERNEL_NO_MEMORY = unchecked((int) 0x80020190),
        ERROR_KERNEL_ILLEGAL_ATTR = unchecked((int) 0x80020191),
        ERROR_KERNEL_ILLEGAL_THREAD_ENTRY_ADDR = unchecked((int) 0x80020192),
        ERROR_KERNEL_ILLEGAL_PRIORITY = unchecked((int) 0x80020193),
        ERROR_KERNEL_ILLEGAL_STACK_SIZE = unchecked((int) 0x80020194),
        ERROR_KERNEL_ILLEGAL_MODE = unchecked((int) 0x80020195),
        ERROR_KERNEL_ILLEGAL_MASK = unchecked((int) 0x80020196),
        ERROR_KERNEL_ILLEGAL_THREAD = unchecked((int) 0x80020197),
        ERROR_KERNEL_NOT_FOUND_THREAD = unchecked((int) 0x80020198),
        ERROR_KERNEL_NOT_FOUND_SEMAPHORE = unchecked((int) 0x80020199),

        ERROR_KERNEL_NOT_FOUND_EVENT_FLAG = unchecked((int) 0x8002019a),

        ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX = unchecked((int) 0x8002019b),
        ERROR_KERNEL_NOT_FOUND_VPOOL = unchecked((int) 0x8002019c),
        ERROR_KERNEL_NOT_FOUND_FPOOL = unchecked((int) 0x8002019d),
        ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE = unchecked((int) 0x8002019e),
        ERROR_KERNEL_NOT_FOUND_ALARM = unchecked((int) 0x8002019f),
        ERROR_KERNEL_NOT_FOUND_THREAD_EVENT_HANDLER = unchecked((int) 0x800201a0),
        ERROR_KERNEL_NOT_FOUND_CALLBACK = unchecked((int) 0x800201a1),
        ERROR_KERNEL_THREAD_ALREADY_DORMANT = unchecked((int) 0x800201a2),
        ERROR_KERNEL_THREAD_ALREADY_SUSPEND = unchecked((int) 0x800201a3),
        ERROR_KERNEL_THREAD_IS_NOT_DORMANT = unchecked((int) 0x800201a4),
        ERROR_KERNEL_THREAD_IS_NOT_SUSPEND = unchecked((int) 0x800201a5),
        ERROR_KERNEL_THREAD_IS_NOT_WAIT = unchecked((int) 0x800201a6),
        ERROR_KERNEL_WAIT_CAN_NOT_WAIT = unchecked((int) 0x800201a7),
        ERROR_KERNEL_WAIT_TIMEOUT = unchecked((int) 0x800201a8),
        ERROR_KERNEL_WAIT_CANCELLED = unchecked((int) 0x800201a9),
        ERROR_KERNEL_WAIT_STATUS_RELEASED = unchecked((int) 0x800201aa),
        ERROR_KERNEL_WAIT_STATUS_RELEASED_CALLBACK = unchecked((int) 0x800201ab),
        ERROR_KERNEL_THREAD_IS_TERMINATED = unchecked((int) 0x800201ac),
        ERROR_KERNEL_SEMA_ZERO = unchecked((int) 0x800201ad),
        ERROR_KERNEL_SEMA_OVERFLOW = unchecked((int) 0x800201ae),

        ERROR_KERNEL_EVENT_FLAG_POLL_FAILED = unchecked((int) 0x800201af),
        ERROR_KERNEL_EVENT_FLAG_NO_MULTI_PERM = unchecked((int) 0x800201b0),
        ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN = unchecked((int) 0x800201b1),

        ERROR_KERNEL_MESSAGEBOX_NO_MESSAGE = unchecked((int) 0x800201b2),
        ERROR_KERNEL_MESSAGE_PIPE_FULL = unchecked((int) 0x800201b3),
        ERROR_KERNEL_MESSAGE_PIPE_EMPTY = unchecked((int) 0x800201b4),
        ERROR_KERNEL_WAIT_DELETE = unchecked((int) 0x800201b5),
        ERROR_KERNEL_ILLEGAL_MEMBLOCK = unchecked((int) 0x800201b6),
        ERROR_KERNEL_ILLEGAL_MEMSIZE = unchecked((int) 0x800201b7),
        ERROR_KERNEL_ILLEGAL_SCRATCHPAD_ADDR = unchecked((int) 0x800201b8),
        ERROR_KERNEL_SCRATCHPAD_IN_USE = unchecked((int) 0x800201b9),
        ERROR_KERNEL_SCRATCHPAD_NOT_IN_USE = unchecked((int) 0x800201ba),
        ERROR_KERNEL_ILLEGAL_TYPE = unchecked((int) 0x800201bb),
        ERROR_KERNEL_ILLEGAL_SIZE = unchecked((int) 0x800201bc),
        ERROR_KERNEL_ILLEGAL_COUNT = unchecked((int) 0x800201bd),
        ERROR_KERNEL_NOT_FOUND_VTIMER = unchecked((int) 0x800201be),
        ERROR_KERNEL_ILLEGAL_VTIMER = unchecked((int) 0x800201bf),
        ERROR_KERNEL_ILLEGAL_KTLS = unchecked((int) 0x800201c0),
        ERROR_KERNEL_KTLS_IS_FULL = unchecked((int) 0x800201c1),
        ERROR_KERNEL_KTLS_IS_BUSY = unchecked((int) 0x800201c2),
        ERROR_KERNEL_MUTEX_NOT_FOUND = unchecked((int) 0x800201c3),
        ERROR_KERNEL_MUTEX_LOCKED = unchecked((int) 0x800201c4),
        ERROR_KERNEL_MUTEX_UNLOCKED = unchecked((int) 0x800201c5),
        ERROR_KERNEL_MUTEX_LOCK_OVERFLOW = unchecked((int) 0x800201c6),
        ERROR_KERNEL_MUTEX_UNLOCK_UNDERFLOW = unchecked((int) 0x800201c7),
        ERROR_KERNEL_MUTEX_RECURSIVE_NOT_ALLOWED = unchecked((int) 0x800201c8),
        ERROR_KERNEL_MESSAGEBOX_DUPLICATE_MESSAGE = unchecked((int) 0x800201c9),

        //PSP_LWMUTEX_ERROR_NO_SUCH_LWMUTEX 0x800201CA
        //PSP_LWMUTEX_ERROR_TRYLOCK_FAILED 0x800201CB
        //PSP_LWMUTEX_ERROR_NOT_LOCKED 0x800201CC
        //PSP_LWMUTEX_ERROR_LOCK_OVERFLOW 0x800201CD
        //PSP_LWMUTEX_ERROR_UNLOCK_UNDERFLOW 0x800201CE
        //PSP_LWMUTEX_ERROR_ALREADY_LOCKED 0x800201CF


        ERROR_KERNEL_LWMUTEX_NOT_FOUND = unchecked((int) 0x800201ca),
        ERROR_KERNEL_LWMUTEX_LOCKED = unchecked((int) 0x800201cb),
        ERROR_KERNEL_LWMUTEX_UNLOCKED = unchecked((int) 0x800201cc),
        ERROR_KERNEL_LWMUTEX_LOCK_OVERFLOW = unchecked((int) 0x800201cd),
        ERROR_KERNEL_LWMUTEX_UNLOCK_UNDERFLOW = unchecked((int) 0x800201ce),
        ERROR_KERNEL_LWMUTEX_RECURSIVE_NOT_ALLOWED = unchecked((int) 0x800201cf),

        ERROR_KERNEL_POWER_CANNOT_CANCEL = unchecked((int) 0x80020261),

        ERROR_KERNEL_TOO_MANY_OPEN_FILES = unchecked((int) 0x80020320),
        ERROR_KERNEL_NO_SUCH_DEVICE = unchecked((int) 0x80020321),
        ERROR_KERNEL_BAD_FILE_DESCRIPTOR = unchecked((int) 0x80020323),
        ERROR_KERNEL_UNSUPPORTED_OPERATION = unchecked((int) 0x80020325),
        ERROR_KERNEL_NOCWD = unchecked((int) 0x8002032c),
        ERROR_KERNEL_FILENAME_TOO_LONG = unchecked((int) 0x8002032d),
        ERROR_KERNEL_ASYNC_BUSY = unchecked((int) 0x80020329),
        ERROR_KERNEL_NO_ASYNC_OP = unchecked((int) 0x8002032a),

        ERROR_KERNEL_NOT_CACHE_ALIGNED = unchecked((int) 0x8002044c),
        ERROR_KERNEL_MAX_ERROR = unchecked((int) 0x8002044d),

        ERROR_UTILITY_INVALID_STATUS = unchecked((int) 0x80110001),
        ERROR_UTILITY_INVALID_PARAM_ADDR = unchecked((int) 0x80110002),
        ERROR_UTILITY_IS_UNKNOWN = unchecked((int) 0x80110003),
        ERROR_UTILITY_INVALID_PARAM_SIZE = unchecked((int) 0x80110004),
        ERROR_UTILITY_WRONG_TYPE = unchecked((int) 0x80110005),
        ERROR_UTILITY_MODULE_NOT_FOUND = unchecked((int) 0x80110006),

        ERROR_SAVEDATA_LOAD_NO_MEMSTICK = unchecked((int) 0x80110301),
        ERROR_SAVEDATA_LOAD_MEMSTICK_REMOVED = unchecked((int) 0x80110302),
        ERROR_SAVEDATA_LOAD_ACCESS_ERROR = unchecked((int) 0x80110305),
        ERROR_SAVEDATA_LOAD_DATA_BROKEN = unchecked((int) 0x80110306),
        ERROR_SAVEDATA_LOAD_NO_DATA = unchecked((int) 0x80110307),
        ERROR_SAVEDATA_LOAD_BAD_PARAMS = unchecked((int) 0x80110308),
        ERROR_SAVEDATA_LOAD_NO_UMD = unchecked((int) 0x80110309),
        ERROR_SAVEDATA_LOAD_INTERNAL_ERROR = unchecked((int) 0x80110309),

        ERROR_SAVEDATA_RW_NO_MEMSTICK = unchecked((int) 0x80110321),
        ERROR_SAVEDATA_RW_MEMSTICK_REMOVED = unchecked((int) 0x80110322),
        ERROR_SAVEDATA_RW_MEMSTICK_FULL = unchecked((int) 0x80110323),
        ERROR_SAVEDATA_RW_MEMSTICK_PROTECTED = unchecked((int) 0x80110324),
        ERROR_SAVEDATA_RW_ACCESS_ERROR = unchecked((int) 0x80110325),
        ERROR_SAVEDATA_RW_DATA_BROKEN = unchecked((int) 0x80110326),
        ERROR_SAVEDATA_RW_NO_DATA = unchecked((int) 0x80110327),
        ERROR_SAVEDATA_RW_BAD_PARAMS = unchecked((int) 0x80110328),
        ERROR_SAVEDATA_RW_FILE_NOT_FOUND = unchecked((int) 0x80110329),
        ERROR_SAVEDATA_RW_CAN_NOT_SUSPEND = unchecked((int) 0x8011032a),
        ERROR_SAVEDATA_RW_INTERNAL_ERROR = unchecked((int) 0x8011032b),
        ERROR_SAVEDATA_RW_BAD_STATUS = unchecked((int) 0x8011032c),
        ERROR_SAVEDATA_RW_SECURE_FILE_FULL = unchecked((int) 0x8011032d),

        ERROR_SAVEDATA_DELETE_NO_MEMSTICK = unchecked((int) 0x80110341),
        ERROR_SAVEDATA_DELETE_MEMSTICK_REMOVED = unchecked((int) 0x80110342),
        ERROR_SAVEDATA_DELETE_MEMSTICK_PROTECTED = unchecked((int) 0x80110344),
        ERROR_SAVEDATA_DELETE_ACCESS_ERROR = unchecked((int) 0x80110345),
        ERROR_SAVEDATA_DELETE_DATA_BROKEN = unchecked((int) 0x80110346),
        ERROR_SAVEDATA_DELETE_NO_DATA = unchecked((int) 0x80110347),
        ERROR_SAVEDATA_DELETE_BAD_PARAMS = unchecked((int) 0x80110348),
        ERROR_SAVEDATA_DELETE_INTERNAL_ERROR = unchecked((int) 0x8011034b),

        ERROR_SAVEDATA_SAVE_NO_MEMSTICK = unchecked((int) 0x80110381),
        ERROR_SAVEDATA_SAVE_MEMSTICK_REMOVED = unchecked((int) 0x80110382),
        ERROR_SAVEDATA_SAVE_NO_SPACE = unchecked((int) 0x80110383),
        ERROR_SAVEDATA_SAVE_MEMSTICK_PROTECTED = unchecked((int) 0x80110384),
        ERROR_SAVEDATA_SAVE_ACCESS_ERROR = unchecked((int) 0x80110385),
        ERROR_SAVEDATA_SAVE_BAD_PARAMS = unchecked((int) 0x80110388),
        ERROR_SAVEDATA_SAVE_NO_UMD = unchecked((int) 0x80110389),
        ERROR_SAVEDATA_SAVE_WRONG_UMD = unchecked((int) 0x8011038a),
        ERROR_SAVEDATA_SAVE_INTERNAL_ERROR = unchecked((int) 0x8011038b),

        ERROR_SAVEDATA_SIZES_NO_MEMSTICK = unchecked((int) 0x801103c1),
        ERROR_SAVEDATA_SIZES_MEMSTICK_REMOVED = unchecked((int) 0x801103c2),
        ERROR_SAVEDATA_SIZES_ACCESS_ERROR = unchecked((int) 0x801103c5),
        ERROR_SAVEDATA_SIZES_DATA_BROKEN = unchecked((int) 0x801103c6),
        ERROR_SAVEDATA_SIZES_NO_DATA = unchecked((int) 0x801103c7),
        ERROR_SAVEDATA_SIZES_BAD_PARAMS = unchecked((int) 0x801103c8),
        ERROR_SAVEDATA_SIZES_INTERNAL_ERROR = unchecked((int) 0x801103cb),

        ERROR_NETPARAM_BAD_NETCONF = unchecked((int) 0x80110601),
        ERROR_NETPARAM_BAD_PARAM = unchecked((int) 0x80110604),

        ERROR_NET_MODULE_BAD_ID = unchecked((int) 0x80110801),
        ERROR_NET_MODULE_ALREADY_LOADED = unchecked((int) 0x80110802),
        ERROR_NET_MODULE_NOT_LOADED = unchecked((int) 0x80110803),

        ERROR_AV_MODULE_BAD_ID = unchecked((int) 0x80110901),
        ERROR_AV_MODULE_ALREADY_LOADED = unchecked((int) 0x80110902),
        ERROR_AV_MODULE_NOT_LOADED = unchecked((int) 0x80110903),

        ERROR_MODULE_BAD_ID = unchecked((int) 0x80111101),
        ERROR_MODULE_ALREADY_LOADED = unchecked((int) 0x80111102),
        ERROR_MODULE_NOT_LOADED = unchecked((int) 0x80111103),

        ERROR_SCREENSHOT_CONT_MODE_NOT_INIT = unchecked((int) 0x80111229),

        ERROR_UMD_NOT_READY = unchecked((int) 0x80210001),
        ERROR_UMD_LBA_OUT_OF_BOUNDS = unchecked((int) 0x80210002),
        ERROR_UMD_NO_DISC = unchecked((int) 0x80210003),

        ERROR_MEMSTICK_DEVCTL_BAD_PARAMS = unchecked((int) 0x80220081),
        ERROR_MEMSTICK_DEVCTL_TOO_MANY_CALLBACKS = unchecked((int) 0x80220082),

        ERROR_AUDIO_CHANNEL_NOT_INIT = unchecked((int) 0x80260001),
        ERROR_AUDIO_CHANNEL_BUSY = unchecked((int) 0x80260002),
        ERROR_AUDIO_INVALID_CHANNEL = unchecked((int) 0x80260003),
        ERROR_AUDIO_PRIV_REQUIRED = unchecked((int) 0x80260004),
        ERROR_AUDIO_NO_CHANNELS_AVAILABLE = unchecked((int) 0x80260005),
        ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED = unchecked((int) 0x80260006),
        ERROR_AUDIO_INVALID_FORMAT = unchecked((int) 0x80260007),
        ERROR_AUDIO_CHANNEL_NOT_RESERVED = unchecked((int) 0x80260008),
        ERROR_AUDIO_NOT_OUTPUT = unchecked((int) 0x80260009),

        ERROR_POWER_VMEM_IN_USE = unchecked((int) 0x802b0200),

        ERROR_NET_RESOLVER_BAD_ID = unchecked((int) 0x80410408),
        ERROR_NET_RESOLVER_ALREADY_STOPPED = unchecked((int) 0x8041040a),
        ERROR_NET_RESOLVER_INVALID_HOST = unchecked((int) 0x80410414),

        ERROR_WLAN_BAD_PARAMS = unchecked((int) 0x80410d13),

        ERROR_HTTP_NOT_INIT = unchecked((int) 0x80431001),
        ERROR_HTTP_ALREADY_INIT = unchecked((int) 0x80431020),
        ERROR_HTTP_NO_MEMORY = unchecked((int) 0x80431077),
        ERROR_HTTP_SYSTEM_COOKIE_NOT_LOADED = unchecked((int) 0x80431078),
        ERROR_HTTP_INVALID_PARAMETER = unchecked((int) 0x804311FE),

        ERROR_SSL_NOT_INIT = unchecked((int) 0x80435001),
        ERROR_SSL_ALREADY_INIT = unchecked((int) 0x80435020),
        ERROR_SSL_OUT_OF_MEMORY = unchecked((int) 0x80435022),
        ERROR_HTTPS_CERT_ERROR = unchecked((int) 0x80435060),
        ERROR_HTTPS_HANDSHAKE_ERROR = unchecked((int) 0x80435061),
        ERROR_HTTPS_IO_ERROR = unchecked((int) 0x80435062),
        ERROR_HTTPS_INTERNAL_ERROR = unchecked((int) 0x80435063),
        ERROR_HTTPS_PROXY_ERROR = unchecked((int) 0x80435064),
        ERROR_SSL_INVALID_PARAMETER = unchecked((int) 0x804351FE),

        ERROR_WAVE_NOT_INIT = unchecked((int) 0x80440001),
        ERROR_WAVE_FAILED_EXIT = unchecked((int) 0x80440002),
        ERROR_WAVE_BAD_VOL = unchecked((int) 0x8044000a),
        ERROR_WAVE_INVALID_CHANNEL = unchecked((int) 0x80440010),
        ERROR_WAVE_INVALID_SAMPLE_COUNT = unchecked((int) 0x80440011),

        ERROR_FONT_INVALID_LIBID = unchecked((int) 0x80460002),
        ERROR_FONT_INVALID_PARAMETER = unchecked((int) 0x80460003),
        ERROR_FONT_TOO_MANY_OPEN_FONTS = unchecked((int) 0x80460009),

        ERROR_MPEG_BAD_VERSION = unchecked((int) 0x80610002),
        ERROR_MPEG_NO_MEMORY = unchecked((int) 0x80610022),
        ERROR_MPEG_INVALID_ADDR = unchecked((int) 0x80610103),
        ERROR_MPEG_INVALID_VALUE = unchecked((int) 0x806101fe),

        ERROR_PSMF_NOT_INITIALIZED = unchecked((int) 0x80615001),
        ERROR_PSMF_BAD_VERSION = unchecked((int) 0x80615002),
        ERROR_PSMF_NOT_FOUND = unchecked((int) 0x80615025),
        ERROR_PSMF_INVALID_ID = unchecked((int) 0x80615100),
        ERROR_PSMF_INVALID_VALUE = unchecked((int) 0x806151fe),
        ERROR_PSMF_INVALID_TIMESTAMP = unchecked((int) 0x80615500),
        ERROR_PSMF_INVALID_PSMF = unchecked((int) 0x80615501),

        ERROR_PSMFPLAYER_NOT_INITIALIZED = unchecked((int) 0x80616001),
        ERROR_PSMFPLAYER_NO_MORE_DATA = unchecked((int) 0x8061600c),

        ERROR_MPEG_NO_DATA = unchecked((int) 0x80618001),

        ERROR_AVC_VIDEO_FATAL = unchecked((int) 0x80628002),

        ERROR_ATRAC_NO_ID = unchecked((int) 0x80630003),
        ERROR_ATRAC_INVALID_CODEC = unchecked((int) 0x80630004),
        ERROR_ATRAC_BAD_ID = unchecked((int) 0x80630005),
        ERROR_ATRAC_ALL_DATA_LOADED = unchecked((int) 0x80630009),
        ERROR_ATRAC_NO_DATA = unchecked((int) 0x80630010),
        ERROR_ATRAC_SECOND_BUFFER_NEEDED = unchecked((int) 0x80630012),
        ERROR_ATRAC_SECOND_BUFFER_NOT_NEEDED = unchecked((int) 0x80630022),
        ERROR_ATRAC_BUFFER_IS_EMPTY = unchecked((int) 0x80630023),
        ERROR_ATRAC_ALL_DATA_DECODED = unchecked((int) 0x80630024),

        ERROR_CODEC_AUDIO_FATAL = unchecked((int) 0x807f00fc),

        FATAL_UMD_UNKNOWN_MEDIUM = unchecked((int) 0xC0210004),
        FATAL_UMD_HARDWARE_FAILURE = unchecked((int) 0xC0210005),

        //ERROR_AUDIO_CHANNEL_NOT_INIT                        = unchecked((int)0x80260001),
        //ERROR_AUDIO_CHANNEL_BUSY                            = unchecked((int)0x80260002),
        //ERROR_AUDIO_INVALID_CHANNEL                         = unchecked((int)0x80260003),
        //ERROR_AUDIO_PRIV_REQUIRED                           = unchecked((int)0x80260004),
        //ERROR_AUDIO_NO_CHANNELS_AVAILABLE                   = unchecked((int)0x80260005),
        //ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED     = unchecked((int)0x80260006),
        //ERROR_AUDIO_INVALID_FORMAT                          = unchecked((int)0x80260007),
        //ERROR_AUDIO_CHANNEL_NOT_RESERVED                    = unchecked((int)0x80260008),
        //ERROR_AUDIO_NOT_OUTPUT                              = unchecked((int)0x80260009),
        ERROR_AUDIO_INVALID_FREQUENCY = unchecked((int) 0x8026000A),
        ERROR_AUDIO_INVALID_VOLUME = unchecked((int) 0x8026000B),
        ERROR_AUDIO_CHANNEL_ALREADY_RESERVED = unchecked((int) 0x80268002),
        PSP_AUDIO_ERROR_SRC_FORMAT_4 = unchecked((int) 0x80000003),

        ATRAC_ERROR_API_FAIL = unchecked((int) 0x80630002),
        ATRAC_ERROR_NO_ATRACID = unchecked((int) 0x80630003),
        ATRAC_ERROR_INVALID_CODECTYPE = unchecked((int) 0x80630004),
        ATRAC_ERROR_BAD_ATRACID = unchecked((int) 0x80630005),
        ATRAC_ERROR_ALL_DATA_LOADED = unchecked((int) 0x80630009),
        ATRAC_ERROR_NO_DATA = unchecked((int) 0x80630010),
        ATRAC_ERROR_SECOND_BUFFER_NEEDED = unchecked((int) 0x80630012),
        ATRAC_ERROR_INCORRECT_READ_SIZE = unchecked((int) 0x80630013),
        ATRAC_ERROR_ADD_DATA_IS_TOO_BIG = unchecked((int) 0x80630018),
        ATRAC_ERROR_UNSET_PARAM = unchecked((int) 0x80630021),
        ATRAC_ERROR_SECOND_BUFFER_NOT_NEEDED = unchecked((int) 0x80630022),
        ATRAC_ERROR_BUFFER_IS_EMPTY = unchecked((int) 0x80630023),
        ATRAC_ERROR_ALL_DATA_DECODED = unchecked((int) 0x80630024),

        PSP_SYSTEMPARAM_RETVAL = unchecked((int) 0x80110103),

        ERROR_SAS_INVALID_VOICE = unchecked((int) 0x80420010),
        ERROR_SAS_INVALID_ADSR_CURVE_MODE = unchecked((int) 0x80420013),
        ERROR_SAS_INVALID_PARAMETER = unchecked((int) 0x80420014),
        ERROR_SAS_VOICE_PAUSED = unchecked((int) 0x80420016),
        ERROR_SAS_BUSY = unchecked((int) 0x80420030),
        ERROR_SAS_NOT_INIT = unchecked((int) 0x80420100),

        ERROR_SAS_INVALID_GRAIN = unchecked((int) 0x80420001),
        ERROR_SAS_INVALID_MAX_VOICES = unchecked((int) 0x80420002),
        ERROR_SAS_INVALID_OUTPUT_MODE = unchecked((int) 0x80420003),
        ERROR_SAS_INVALID_SAMPLE_RATE = unchecked((int) 0x80420004),
        ERROR_SAS_INVALID_ADDRESS = unchecked((int) 0x80420005),
        ERROR_SAS_INVALID_VOICE_INDEX = unchecked((int) 0x80420010),
        ERROR_SAS_INVALID_NOISE_CLOCK = unchecked((int) 0x80420011),
        ERROR_SAS_INVALID_PITCH_VAL = unchecked((int) 0x80420012),

        //ERROR_SAS_INVALID_ADSR_CURVE_MODE                   = unchecked((int)0x80420013),
        ERROR_SAS_INVALID_ADPCM_SIZE = unchecked((int) 0x80420014),
        ERROR_SAS_INVALID_LOOP_MODE = unchecked((int) 0x80420015),

        //ERROR_SAS_VOICE_PAUSED                              = unchecked((int)0x80420016),
        ERROR_SAS_INVALID_VOLUME_VAL = unchecked((int) 0x80420018),
        ERROR_SAS_INVALID_ADSR_VAL = unchecked((int) 0x80420019),
        ERROR_SAS_INVALID_SIZE = unchecked((int) 0x8042001A),
        ERROR_SAS_INVALID_FX_TYPE = unchecked((int) 0x80420020),
        ERROR_SAS_INVALID_FX_FEEDBACK = unchecked((int) 0x80420021),
        ERROR_SAS_INVALID_FX_DELAY = unchecked((int) 0x80420022),
        ERROR_SAS_INVALID_FX_VOLUME_VAL = unchecked((int) 0x80420023),

        //ERROR_SAS_BUSY                                      = unchecked((int)0x80420030),
        //ERROR_SAS_NOT_INIT                                  = unchecked((int)0x80420100),
        ERROR_SAS_ALREADY_INIT = unchecked((int) 0x80420101),

        PSP_POWER_ERROR_TAKEN_SLOT = unchecked((int) 0x80000020),
        PSP_POWER_ERROR_SLOTS_FULL = unchecked((int) 0x80000022),
        PSP_POWER_ERROR_PRIVATE_SLOT = unchecked((int) 0x80000023),
        PSP_POWER_ERROR_EMPTY_SLOT = unchecked((int) 0x80000025),
        PSP_POWER_ERROR_INVALID_CB = unchecked((int) 0x80000100),
        PSP_POWER_ERROR_INVALID_SLOT = unchecked((int) 0x80000102),
    }
}