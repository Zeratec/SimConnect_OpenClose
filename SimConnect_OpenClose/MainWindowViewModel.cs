using SimConnect_OpenClose.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using System.Windows.Threading;

namespace SimConnect_OpenClose
{
    class MainWindowViewModel
    {
        #region Variable
        // User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        // Window handle
        private IntPtr m_hWnd = new IntPtr(0);

        // SimConnect object
        private SimConnect m_oSimConnect = null;

        private DispatcherTimer m_oTimer = new DispatcherTimer();

        public ICommand cmdToggleConnect { get; private set; }
        #endregion Variable

        #region Constructor
        public MainWindowViewModel()
        {
            cmdToggleConnect = new RelayCommand(ToggleConnect);

            m_oTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            m_oTimer.Tick += new EventHandler(OnTick);
        }
        #endregion Constructor

        #region Public method
        public bool bOddTick
        {
            get { return m_bOddTick; }
            set { this.SetProperty(ref m_bOddTick, value); }
        }
        #endregion Public method

        #region Private method
        private void ToggleConnect(object commandParameter)
        {
            if (m_oSimConnect == null)
            {
                try
                {
                    Connect();
                }
                catch (COMException ex)
                {
                    Console.WriteLine("Unable to connect to KH: " + ex.Message);
                }
            }
            else
            {
                Disconnect();
            }
        }

        private void Connect()
        {
            try
            {
                // The constructor is similar to SimConnect_Open in the native API
                m_oSimConnect = new SimConnect("Simconnect - Simvar test", m_hWnd, WM_USER_SIMCONNECT, null, 0);
                Console.WriteLine("Connect");

                // Listen to connect and quit msgs
                m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                // Listen to exceptions
                m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                // Catch a simobject data request
             //   m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
            }
            catch (COMException ex)
            {
                Console.WriteLine("Connection to KH failed: " + ex.Message);
            }
        }

        private void Disconnect()
        {
            Console.WriteLine("Disconnect");

            //m_oTimer.Stop();
            //bOddTick = false;

            if (m_oSimConnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                m_oSimConnect.Dispose();
                m_oSimConnect = null;
            }

            //sConnectButtonLabel = "Connect";
            //bConnected = false;

            //// Set all requests as pending
            //foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            //{
            //    oSimvarRequest.bPending = true;
            //    oSimvarRequest.bStillPending = true;
            //}
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect_OnRecvOpen");
            Console.WriteLine("Connected to KH");

            //sConnectButtonLabel = "Disconnect";
            //bConnected = true;

            //// Register pending requests
            //foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            //{
            //    if (oSimvarRequest.bPending)
            //    {
            //        oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            //        oSimvarRequest.bStillPending = oSimvarRequest.bPending;
            //    }
            //}

            m_oTimer.Start();
            bOddTick = false;
        }

        // May not be the best way to achive regular requests.
        // See SimConnect.RequestDataOnSimObject
        private void OnTick(object sender, EventArgs e)
        {
            Console.WriteLine("OnTick");

            bOddTick = !bOddTick;

            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (!oSimvarRequest.bPending)
                {
                    m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, m_eSimObjectType);
                    oSimvarRequest.bPending = true;
                }
                else
                {
                    oSimvarRequest.bStillPending = true;
                }
            }
        }
        #endregion Private method
    }
}
