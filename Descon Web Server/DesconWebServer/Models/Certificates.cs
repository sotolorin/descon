using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DesconWebServer.Models
{
	public class Certificates
	{
		/// <summary>
		/// The certificate must be located on the server and installed for all users. 
		/// The certificate can be found in TFS at: Descon8\Docs and Reference\mail.desconplus.com.cer
		/// If the certificate changes or needs to be renewed, go the https://mail.desconplus.com/ and export it from your browser
		/// </summary>
		public static bool CertificateValidation(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			const SslPolicyErrors ignoredErrors =
				SslPolicyErrors.RemoteCertificateChainErrors | // self-signed
				SslPolicyErrors.RemoteCertificateNameMismatch; // name mismatch

			if ((sslPolicyErrors & ~ignoredErrors) == SslPolicyErrors.None)
				return true;
			else
				return false;
		}
	}
}