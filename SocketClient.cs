using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WinSockClient
{


    public class SocketClient
    {

        #region "VARIABLES"
        private Stream Stm;

        private Hashtable Conexiones = new Hashtable();
        private int _lengBufferSocket = 4000;

        #endregion

        #region "EVENTOS"
        public event ConexionTerminadaEventHandler ConexionTerminada;
        public delegate void ConexionTerminadaEventHandler();
        public event DatosRecibidosNEventHandler DatosRecibidos;
        public delegate void DatosRecibidosNEventHandler(string datos, IPEndPoint IDCliente);


        #endregion

        #region "PROPIEDADES"
        public string IPDelHost { get; set; }

        public string PuertoDelHost { get; set; }

        public int LengBufferSocket
        {
            get => _lengBufferSocket;
            set => _lengBufferSocket = value;
        }

        #endregion

        #region "METODOS"
        public void Conectar()
        {
            IPAddress address = IPAddress.Parse(IPDelHost);
            TcpClient tcpClnt = new TcpClient();
            tcpClnt.Connect(address, Convert.ToInt32(PuertoDelHost));
            Stm = tcpClnt.GetStream();
            Conexiones.Add(tcpClnt.Client.RemoteEndPoint, Stm);
            var tcpThd = new Thread(LeerSocket);
            tcpThd.Start();
        }

        public void SendData(IPEndPoint IDCliente, string Datos)
        {
            Stream stmN = (Stream)Conexiones[IDCliente];
            if (stmN == null) return;
            byte[] bufferDeEscritura = Encoding.ASCII.GetBytes(Datos);
            stmN.Write(bufferDeEscritura, 0, bufferDeEscritura.Length);
        }

        public void SendData(string Datos)
        {
            if (Stm == null) return;
            byte[] bufferDeEscritura = Encoding.ASCII.GetBytes(Datos);
            Stm.Write(bufferDeEscritura, 0, bufferDeEscritura.Length);

        }

        #endregion

        #region "FUNCIONES PRIVADAS"
        public void LeerSocket()
        {
            byte[] bufferDeLectura = null;
            try
            {
                while (true)
                {
                    bufferDeLectura = new byte[_lengBufferSocket];

                    int cantBytes = Stm.Read(bufferDeLectura, 0, bufferDeLectura.Length);
                    if (cantBytes > 0)
                    {
                        bufferDeLectura = bufferDeLectura.Take(cantBytes).ToArray();
                        if (DatosRecibidos != null)
                        {
                            IPAddress da = IPAddress.Parse(IPDelHost);
                            IPEndPoint client = new System.Net.IPEndPoint(da, Convert.ToInt32(PuertoDelHost));

                            DatosRecibidos(Encoding.ASCII.GetString(bufferDeLectura), client);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                //Finalizo la conexion, por lo tanto genero el evento correspondiente
                if (ConexionTerminada != null)
                {
                    ConexionTerminada();
                }
            }



        }


        #endregion

    }
}
