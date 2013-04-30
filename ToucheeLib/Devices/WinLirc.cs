using System;
using System.IO;
using System.Net.Sockets;

namespace Touchee.Devices {

    /// <summary>
    /// WinLirc client
    /// </summary>
    public class WinLirc : Base {


        #region Privates

        TcpClient _client;
        NetworkStream _stream;
        StreamWriter _streamWriter;

        #endregion



        #region Singleton


        /// <summary>
        /// Private constructor
        /// </summary>
        WinLirc() {
            this.Connect();
        }


        /// <summary>
        /// The singleton instance of the library
        /// </summary>
        public static WinLirc Client = new WinLirc();


        #endregion



        #region Connecting


        /// <summary>
        /// Connects to the WinLirc server
        /// </summary>
        /// <returns>True if connected, false otherwise</returns>
        public bool Connect() {

            try {
                _client = new TcpClient("127.0.0.1", 8765);
                _stream = _client.GetStream();
                _streamWriter = new StreamWriter(_stream);
                _streamWriter.AutoFlush = true;
                Log("Connected to WinLirc", Logger.LogLevel.Info);
                return true;
            }
            catch (Exception) {
                _client = null;
                _stream = null;
                _streamWriter = null;
                Log("Could not connect to WinLirc. Is it running?", Logger.LogLevel.Error);
                return false;
            }

        }


        /// <summary>
        /// Disconnects from the WinLirc server
        /// </summary>
        public void Disconnect() {
            if (_streamWriter != null) {
                _streamWriter.Close();
                _stream.Close();
                _client.Close();
            }
        }


        #endregion



        #region Sending


        /// <summary>
        /// Send a command to WinLirc.
        /// If the command cannot be sent, this method tries to (re-)connect to the WinLirc server once.
        /// </summary>
        /// <param name="remote">The remote parameter to send</param>
        /// <param name="command">The command parameter to send</param>
        public bool SendOnce(string remote, string command) {
            var message = String.Format("SEND_ONCE {0} {1}", remote, command);

            // If we can write
            if (_stream != null && _stream.CanWrite) {
                Log("Sending command to WinLirc: " + message, Logger.LogLevel.Info);
                try {
                    _streamWriter.WriteLine(message);
                    return true;
                }
                catch (Exception) { }
            }
            
            // If we come here, no correct send
            if (this.Connect())
                return this.SendOnce(remote, command);
            else {
                Log("Cannot send command to WinLirc. WinLirc not running? Command: " + message, Logger.LogLevel.Info);
                return false;
            }
        }


        #endregion


    }

}
