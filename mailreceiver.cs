/* -----------------------------------------------------------------------------------
 * メールの添付ファイル取得処理 C#.NET
 * ※GMailの場合は、アカウントのセキュリティから「安全性の低いアプリのアクセス」を有効にする
 * -----------------------------------------------------------------------------------
 */

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Mail.MailReceiver {

	// 環境定義
	static class Config {
		public const string MailUserid = "【アカウント】";
		public const string MailPasswd = "【パスワード】";
		public const string MailHostName = "pop.gmail.com";
		public const int MailHostPort = 995;
		public const string MailEncoding = "ISO-2022-JP";
		public const string ReceiveFolder = "data";
	}

	// 受信処理
	public class MailReceiver {

		// 添付ファイルの自動保存処理
		public static void Main () {
			Console.WriteLine ("最新メールを受信中...");
			string[] mails = getLatestReceive ();
			string[] data = getAttachmentData (mails[0]);
			if (data[0] != "") {
				if (saveAttachmentData (data[1], data[0])) {
					Console.WriteLine ("添付ファイル(" + data[1] + ")を保存しました。");
				} else {
					Console.WriteLine ("添付ファイルの保存に失敗しました。");
				}
			} else {
				Console.WriteLine ("添付ファイルがありませんでした。");
			}
		}

		/// メールサーバからメール受信
		public static string[] getLatestReceive () {
			TcpClient client = new TcpClient ();
			client.ReceiveTimeout = 10000;
			client.SendTimeout = 10000;
			SslStream stream = null;
			string[] mails = new string[1];
			string msg = "";
			try {
				client.Connect (Config.MailHostName, Config.MailHostPort);
				stream = new SslStream (client.GetStream ());
				stream.AuthenticateAsClient (Config.MailHostName);
				msg = getReceiveData (stream, "", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
				msg = getReceiveData (stream, "USER " + Config.MailUserid + "\r\n", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
				msg = getReceiveData (stream, "PASS " + Config.MailPasswd + "\r\n", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
				msg = getReceiveData (stream, "STAT " + Config.MailPasswd + "\r\n", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
				int mailsCount = int.Parse (msg.Split (' ') [1]);
				msg = getReceiveData (stream, "RETR " + mailsCount.ToString () + "\r\n", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
				mails[0] = msg;
				msg = getReceiveData (stream, "QUIT" + "\r\n", Config.MailEncoding);
				if (msg.StartsWith ("+") == false) throw new Exception ("エラー:" + msg);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				throw;
			} finally {
				if (stream != null) stream.Close ();
				client.Close ();
			}
			return mails;
		}

		/// メールサーバからメール受信（メッセージ）
		private static String getReceiveData (SslStream stm, String req, String enc) {
			if (req != "") {
				Byte[] sdat;
				sdat = System.Text.Encoding.GetEncoding (enc).GetBytes (req);
				stm.Write (sdat, 0, sdat.Length);
				stm.Flush ();
			}
			String msg = "";
			Byte[] rdat = new Byte[] { };
			Array.Resize<Byte> (ref rdat, 1024 * 1024);
			int l = stm.Read (rdat, 0, rdat.Length);
			if (l > 0) {
				Array.Resize<Byte> (ref rdat, l);
				msg = Encoding.GetEncoding (enc).GetString (rdat);
			} else {
				throw new Exception ("エラー");
			}
			if (msg.StartsWith ("+") == true && req.ToUpper ().StartsWith ("RETR")) {
				do {
					Array.Resize<Byte> (ref rdat, 1024 * 1024);
					l = stm.Read (rdat, 0, rdat.Length);
					if (l > 0) {
						Array.Resize<Byte> (ref rdat, l);
						msg += Encoding.GetEncoding (enc).GetString (rdat);
					} else {
						throw new Exception ("エラー");
					}
				} while (msg.EndsWith ("." + "\r\n") == false);
			}
			return msg;
		}

		/// メールメッセージから添付ファイルを取得
		private static string[] getAttachmentData (string str) {
			string[] attachment = new string[] { "", "" };
			string expression = "X-Attachment-Id:.*\r\n\r\n(?<filedata>.*)\r\n--";
			Regex reg = new Regex (expression, RegexOptions.Singleline);
			Match match = reg.Match (str);
			if (match.Success) {
				attachment[0] = match.Groups["filedata"].Value;
			} else {
				expression = "filename=.*\r\n\r\n(?<filedata>.*)\r\n--";
				reg = new Regex (expression, RegexOptions.Singleline);
				match = reg.Match (str);
				if (match.Success) {
					attachment[0] = match.Groups["filedata"].Value;
				}
			}
			if (attachment[0] != "") {
				expression = "filename=\"(?<filename>.*)\"";
				reg = new Regex (expression, RegexOptions.Singleline);
				match = reg.Match (str);
				if (match.Success) {
					attachment[1] = match.Groups["filename"].Value;
					if (attachment[1].Length > 100) attachment[1] = "attachment";
				}
			}
			return attachment;
		}

		//添付ファイルを保存
		private static bool saveAttachmentData (string filename, string data) {
			bool ret = true;
			byte[] bs = System.Convert.FromBase64String (data);
			string filedir = System.Environment.CurrentDirectory + "\\" + Config.ReceiveFolder;
			if (!Directory.Exists (filedir)) Directory.CreateDirectory (filedir);
			FileStream outFile = new FileStream (filedir + "\\" + filename, FileMode.Create, FileAccess.Write);
			try {
				outFile.Write (bs, 0, bs.Length);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				ret = false;
			} finally {
				outFile.Close ();
			}
			return ret;
		}
	}
}