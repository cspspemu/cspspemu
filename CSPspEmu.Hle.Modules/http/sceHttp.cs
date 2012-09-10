using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.http
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceHttp : HleModuleHost
	{
		/// <summary>
		/// Init the http library.
		/// </summary>
		/// <param name="heapSize">Memory pool size? Pass 20000</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xAB1ABE07, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceHttpInit(int heapSize) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Terminate the http library.
		/// </summary>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xD1C8945E, FirmwareVersion = 150)]
		public int sceHttpEnd() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get http request response length.
		/// </summary>
		/// <param name="requestid">ID of the request created by <see cref="sceHttpCreateRequest"/> or <see cref="sceHttpCreateRequestWithURL"/></param>
		/// <param name="contentlength">The size of the content</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x0282A3BD, FirmwareVersion = 150)]
		public int sceHttpGetContentLength(int requestid, ulong *contentlength){
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Set resolver retry
		/// </summary>
		/// <param name="id">ID of the template or connection </param>
		/// <param name="count">Number of retries</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x03D9526F, FirmwareVersion = 150)]
		public int sceHttpSetResolveRetry(int id, int count) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x06488A1C, FirmwareVersion = 150)]
		public int sceHttpSetCookieSendCallback() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Enable redirect
		/// </summary>
		/// <param name="id">ID of the template or connection </param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x0809C831, FirmwareVersion = 150)]
		public int sceHttpEnableRedirect(int id) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Disable cookie
		/// </summary>
		/// <param name="i">ID of the template or connection </param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x0B12ABFB, FirmwareVersion = 150)]
		public int sceHttpDisableCookie(int i) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Enable cookie
		/// </summary>
		/// <param name="i">ID of the template or connection </param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x0DAFA58F, FirmwareVersion = 150)]
		public int sceHttpEnableCookie(int i) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Delete content header
		/// </summary>
		/// <param name="id">ID of the template, connection or request</param>
		/// <param name="Name">Name of the content</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x15540184, FirmwareVersion = 150)]
		public int sceHttpDeleteHeader(int id, string Name) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Disable redirect
		/// </summary>
		/// <param name="id">ID of the template or connection </param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x1A0EBB69, FirmwareVersion = 150)]
		public int sceHttpDisableRedirect(int id) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x1CEDB9D4, FirmwareVersion = 150)]
		public int sceHttpFlushCache() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Set receive timeout
		/// </summary>
		/// <param name="id">ID of the template or connection </param>
		/// <param name="timeout">Timeout value in microseconds</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x1F0FC3E3, FirmwareVersion = 150)]
		public int sceHttpSetRecvTimeOut(int id, uint timeout) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x2255551E, FirmwareVersion = 150)]
		public int sceHttpGetNetworkPspError() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x267618F4, FirmwareVersion = 150)]
		public int sceHttpSetAuthInfoCallback() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x2A6C3296, FirmwareVersion = 150)]
		public int sceHttpSetAuthInfoCB() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x2C3C82CF, FirmwareVersion = 150)]
		public int sceHttpFlushAuthList() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x3A67F306, FirmwareVersion = 150)]
		public int sceHttpSetCookieRecvCallback() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x3EABA285, FirmwareVersion = 150)]
		public int sceHttpAddExtraHeader() {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Create a http request.
		/// </summary>
		/// <param name="connectionid">ID of the connection created by <see cref="sceHttpCreateConnection"/> or <see cref="sceHttpCreateConnectionWithURL"/></param>
		/// <param name="method">One of <see cref="PspHttpMethod"/></param>
		/// <param name="path">Path to access</param>
		/// <param name="contentlength">Length of the content (POST method only)</param>
		/// <returns>A request ID on success, less than 0 on error.</returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x47347B50, FirmwareVersion = 150)]
		public int sceHttpCreateRequest(int connectionid, PspHttpMethod method, char *path, ulong contentlength) {
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x47940436, FirmwareVersion = 150)]
		public int sceHttpSetResolveTimeOut(int id, uint timeout) {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x4CC7D78F, FirmwareVersion = 150)]
		public int sceHttpGetStatusCode() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x5152773B, FirmwareVersion = 150)]
		public int sceHttpDeleteConnection() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x54E7DF75, FirmwareVersion = 150)]
		public int sceHttpIsRequestInCache() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x59E6D16F, FirmwareVersion = 150)]
		public int sceHttpEnableCache() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x76D1363B, FirmwareVersion = 150)]
		public int sceHttpSaveSystemCookie() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x7774BF4C, FirmwareVersion = 150)]
		public int sceHttpAddCookie() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x77EE5319, FirmwareVersion = 150)]
		public int sceHttpLoadAuthList() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x78A0D3EC, FirmwareVersion = 150)]
		public int sceHttpEnableKeepAlive() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x78B54C09, FirmwareVersion = 150)]
		public int sceHttpEndCache() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x8ACD1F73, FirmwareVersion = 150)]
		public int sceHttpSetConnectTimeOut() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x8EEFD953, FirmwareVersion = 150)]
		public int sceHttpCreateConnection() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x951D310E, FirmwareVersion = 150)]
		public int sceHttpDisableProxyAuth() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x9668864C, FirmwareVersion = 150)]
		public int sceHttpSetRecvBlockSize() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x96F16D3E, FirmwareVersion = 150)]
		public int sceHttpGetCookie() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x9988172D, FirmwareVersion = 150)]
		public int sceHttpSetSendTimeOut() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x9AFC98B2, FirmwareVersion = 150)]
		public int sceHttpSendRequestInCacheFirstMode() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x9B1F1F36, FirmwareVersion = 150)]
		public int sceHttpCreateTemplate() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0x9FC5F10D, FirmwareVersion = 150)]
		public int sceHttpEnableAuth() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xA4496DE5, FirmwareVersion = 150)]
		public int sceHttpSetRedirectCallback() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xA5512E01, FirmwareVersion = 150)]
		public int sceHttpDeleteRequest() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xA6800C34, FirmwareVersion = 150)]
		public int sceHttpInitCache() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xAE948FEE, FirmwareVersion = 150)]
		public int sceHttpDisableAuth() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xB0C34B1D, FirmwareVersion = 150)]
		public int sceHttpSetCacheContentLengthMaxSize() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xB509B09E, FirmwareVersion = 150)]
		public int sceHttpCreateRequestWithURL() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xBB70706F, FirmwareVersion = 150)]
		public int sceHttpSendRequest() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xC10B6BD9, FirmwareVersion = 150)]
		public int sceHttpAbortRequest() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xC6330B0D, FirmwareVersion = 150)]
		public int sceHttpChangeHttpVersion() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xC7EF2559, FirmwareVersion = 150)]
		public int sceHttpDisableKeepAlive() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xC98CBBA7, FirmwareVersion = 150)]
		public int sceHttpSetResHeaderMaxSize() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xCCBD167A, FirmwareVersion = 150)]
		public int sceHttpDisableCache() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xCDB0DC58, FirmwareVersion = 150)]
		public int sceHttpEnableProxyAuth() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xCDF8ECB9, FirmwareVersion = 150)]
		public int sceHttpCreateConnectionWithURL() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xD081EC8F, FirmwareVersion = 150)]
		public int sceHttpGetNetworkErrno() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xD70D4847, FirmwareVersion = 150)]
		public int sceHttpGetProxy() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xDB266CCF, FirmwareVersion = 150)]
		public int sceHttpGetAllHeader() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xDD6E7857, FirmwareVersion = 150)]
		public int sceHttpSaveAuthList() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xEDEEB999, FirmwareVersion = 150)]
		public int sceHttpReadData() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xF0F46C62, FirmwareVersion = 150)]
		public int sceHttpSetProxy() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xF1657B22, FirmwareVersion = 150)]
		public int sceHttpLoadSystemCookie() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xF49934F6, FirmwareVersion = 150)]
		public int sceHttpSetMallocFunction() {
			throw(new NotImplementedException());
		}

		[HlePspNotImplemented]
		[HlePspFunction(NID = 0xFCF8C055, FirmwareVersion = 150)]
		public int sceHttpDeleteTemplate() {
			throw(new NotImplementedException());
		}

		public enum PspHttpMethod {
			Get = 0,
			Post = 1,
			Head = 2,
		}
	}
}
