using System;
using Org.Mentalis.Security.Cryptography;
using System.Security.Cryptography;

public class TestApp {	
	public static void Main(string[] args) {
		ECDiffieHellmanCng alice = new ECDiffieHellmanCng();
		//alice.DeriveKeyMaterial(
		//CngKey.Import(

		// create a new DH instance
		DiffieHellman dh1 = new DiffieHellmanManaged();
		// export the public parameters of the first DH instance
		DHParameters dhp = dh1.ExportParameters(false);
		// create a second DH instance and initialize it with the public parameters of the first instance
		DiffieHellman dh2 = new DiffieHellmanManaged(dhp.P, dhp.G, 160);
		// generate the public key of the first DH instance
		byte[] ke1 = dh1.CreateKeyExchange();
		// generate the public key of the second DH instance
		byte[] ke2 = dh2.CreateKeyExchange();
		// let the first DH instance compute the shared secret using the second DH public key
		byte[] dh1k = dh1.DecryptKeyExchange(ke2);
		// let the second DH instance compute the shared secret using the first DH public key
		byte[] dh2k = dh2.DecryptKeyExchange(ke1);
		// print both shared secrets to verify they are the same
		Console.WriteLine("Computed secret of instance 1:");
		PrintBytes(dh1k);
		Console.WriteLine("\r\nComputed secret of instance 2:");
		PrintBytes(dh2k);

		Console.WriteLine("\r\nPress ENTER to continue...");
		Console.ReadLine();
	}
	private static void PrintBytes(byte[] bytes) {
		if (bytes == null)
			return;
		for(int i = 0; i < bytes.Length; i++) {
			Console.Write(bytes[i].ToString("X2"));
		}
		Console.WriteLine();
	}
}