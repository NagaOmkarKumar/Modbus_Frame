using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace Modbus_Frame
{
    /// <summary>
    /// Interaction logic for LabView.xaml
    /// </summary>
    public partial class View : Window
    {
        private SerialPort serialPort;
        private StringBuilder _sb = new StringBuilder();
        FlowDocument mcFlowDoc = new FlowDocument();
        Paragraph para = new Paragraph();
        public View()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            serialPort = new SerialPort
            {
                PortName = "COM6", // Specify your COM port
                BaudRate = 115200,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            byte slaveId = 0x01;  // Set your slave ID here
            byte functionCode = 0x03;  // Set your function code here (e.g., Read Holding Registers)
            byte[] data = new byte[] { 0x00, 0x01 };  // Set the data bytes (e.g., the register addresses or values)

            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    MessageBox.Show("Serial port opened and ready to write.");
                    SendModbusFrame(slaveId, functionCode, data);  // Start writing data
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void SendModbusFrame(byte slaveId, byte functionCode, byte[] data)
        {
            // Construct Modbus RTU frame
            List<byte> frame = new List<byte>();
            frame.Add(slaveId);             // Slave ID
            frame.Add(functionCode);        // Function Code
            frame.AddRange(data);           // Data bytes (depends on the function)

            // Calculate CRC (Cyclic Redundancy Check) for Modbus RTU frame
            byte[] frameBytes = frame.ToArray();
            byte[] crc = CalculateCRC(frameBytes);
            frame.AddRange(crc);            // Add CRC to the frame

            // Send the frame to the serial port
            serialPort.Write(frame.ToArray(), 0, frame.Count);
        }

        private byte[] CalculateCRC(byte[] frame)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < frame.Length; i++)
            {
                crc ^= frame[i];
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
        }


        private void ProcessReceivedData(byte[] buffer)
        {
            if (buffer.Length < 5)  // Minimum frame length: Slave Address + Function Code + 2 bytes for CRC
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Invalid data received. Frame too short.");
                });
                return;
            }

            byte slaveId = buffer[0];              // Extract slave ID
            byte functionCode = buffer[1];         // Extract function code
            byte[] data = buffer.Skip(2).Take(buffer.Length - 4).ToArray(); // Extract data (excluding CRC)
            int slaveAddress = BitConverter.ToUInt16(new byte[] { data[0], data[1] }, 0);
            // Validate the CRC
            byte[] receivedCrc = buffer.Skip(buffer.Length - 2).Take(2).ToArray();
            byte[] calculatedCrc = CalculateCRC(buffer.Take(buffer.Length - 2).ToArray());

            if (!receivedCrc.SequenceEqual(calculatedCrc))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Invalid CRC for slave {slaveId}, function {functionCode}");
                });
                return;
            }

            // Process based on function code
            if (functionCode == 0x03)  // Read Holding Registers
            {
                // Extract register data (2 bytes per register)
                // Assuming the data represents a 16-bit register (modify as needed)
                foreach (var val in data)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Commdata.AppendText($"Slave {slaveId} Data: {val:X2}\n");
                    });
                }
            }
            else if (functionCode == 0x06)  // Write Holding Register
            {
                // Process the response for a written register (e.g., echo the sent data)
                Dispatcher.Invoke(() =>
                {
                    Commdata.AppendText($"Slave {slaveId} acknowledged write: {BitConverter.ToString(data)}\n");
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Commdata.AppendText($"Received unsupported function code {functionCode} from Slave {slaveId}\n");
                });
            }
        }
        private void Start_Click1(object sender, RoutedEventArgs e)
        {

            serialPort = new SerialPort
            {
                PortName = "COM6", // Specify your COM port
                BaudRate = 115200,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };
            //SerialPort_DataSending();
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    MessageBox.Show("Serial port opened and ready to write.");
                    // Start writing data
                }
               // SerialPort_DataSending();// Begin sending data to the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }

        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                MessageBox.Show("Serial port closed.");
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize the SerialPort object
            serialPort = new SerialPort
            {
                PortName = "COM6", // Specify your COM port
                BaudRate = 115200,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };
            Trace.WriteLine("Connected.");
            // Attach the DataReceived event to read incoming data
            serialPort.DataReceived += SerialPort_DataReceived3;
            //Trace.WriteLine(serialPort.DataReceived);
            try
            {
                serialPort.Open();
                MessageBox.Show("Serial port opened and ready to read.");
                serialPort.DataReceived += SerialPort_DataReceived3;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the serial port when done
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                MessageBox.Show("Serial port closed.");
            }
        }
       
        private void SerialPort_DataReceived3(object sender, SerialDataReceivedEventArgs e)
        {
            Trace.WriteLine("buffer");
            try
            {
                // Create a buffer to hold the incoming data
                byte[] buffer = new byte[serialPort.BytesToRead];
                Trace.WriteLine(buffer);
                // Read the raw byte data from the serial port
                serialPort.Read(buffer, 0, buffer.Length);
                Trace.WriteLine(buffer.Length);
                // Handle the received byte data
                ProcessReceivedData1(buffer);
               // SerialPort_DataSending(buffer);
                SendModbusFrame(buffer[0], buffer[1], buffer);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }

        private void ProcessReceivedData1(byte[] buffer)
        {
            // Process each byte individually or in chunks as needed
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                byte b = buffer[i];

                // Display the byte as a hexadecimal value
                //sb.Append($"0x{b:X2} ");

                // If the byte sequence seems to be a multi-byte number (e.g., integers), we can interpret them
                // For example, assuming 4 bytes per integer, you could do something like:
                //if (i + 3 < buffer.Length)  // Ensure we have enough bytes to form an integer
                //{
                //    int value = BitConverter.ToInt32(buffer, i);
                //    sb.Append($"(int: {value}) ");
                //    i += 3; // Skip ahead after processing an integer (4 bytes)
                //}

                //// If the byte sequence should be interpreted as a float, we could do something like:
                //if (i + 3 < buffer.Length)  // Check if there are enough bytes for a float
                //{
                //    float floatValue = BitConverter.ToSingle(buffer, i);
                //    sb.Append($"(float: {floatValue}) ");
                //    i += 3; // Skip ahead after processing a float (4 bytes)
                //}
            }

            foreach (byte b in buffer)
            {
                // Convert byte to an ASCII character (if it's printable)
                if (b >= 32 && b <= 126) // Printable ASCII range
                {
                    sb.Append((char)b);
                }
                else
                {
                    // Handle non-printable bytes (could be a numeric value or other format)
                    //sb.Append($"0x{b:X2} ({b})");
                    sb.Append($"{b}" + Environment.NewLine);// Display as hexadecimal
                }
            }

            // Update the UI with the processed data
            Dispatcher.Invoke(() =>
            {
                ReceivedDataTextBox.AppendText(sb.ToString() + Environment.NewLine);
            });
        }


        private void SerialPort_DataSending(byte[] buffer)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    // Send the same data byte by byte
                    foreach (byte b in buffer)
                    {
                        byte[] byteToSend = new byte[] { b }; // Create a byte array with a single byte
                        serialPort.Write(byteToSend, 0, 1);  // Send the byte array
                        Thread.Sleep(50); // Optional delay between sending each byte
                    }

                    // Log the sent data in the Commdata RichTextBox
                    StringBuilder sentData = new StringBuilder();
                    foreach (byte b in buffer)
                    {
                        sentData.Append($"{b} ");
                    }

                    // Update the Commdata RichTextBox with the sent data
                    Dispatcher.Invoke(() =>
                    {
                        WriteData($"Sent data: {sentData.ToString()}");
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        WriteData($"Failed to send data: {ex.Message}");
                    });
                }
            }
        }


        // Log the sent data in the Commdata RichTextBox
        private void WriteData(string text)
        {
            Trace.WriteLine(text);
            Commdata.AppendText(text + Environment.NewLine);

            // Assign the value of the plot to the RichTextBox
            para.Inlines.Add(text);
            mcFlowDoc.Blocks.Add(para);
            Commdata.Document = mcFlowDoc;
        }
        //private void SerialPort_DataSending(byte[] buffer)
        //{
        //    if (serialPort.IsOpen)
        //    {
        //        string data = serialPort.ReadLine(); 
        //        Trace.WriteLine(data);
        //        try
        //        {
        //            //string data = serialPort.ReadExisting();
        //            //Send the binary data out the port
        //            byte[] hexstring = Encoding.ASCII.GetBytes(data);
        //            foreach (byte hexval in hexstring)
        //            {
        //                byte[] _hexval = new byte[] { hexval };     // need to convert byte 
        //                                                            // to byte[] to write
        //                serialPort.Write(_hexval, 0, 1);
        //                Thread.Sleep(1);
        //            }
        //            foreach (byte hexval in hexstring)
        //            {
        //                byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
        //                serialPort.Write(_hexval, 0, 1);
        //                Thread.Sleep(1);
        //            }
        //            byte[] data1 = Encoding.ASCII.GetBytes(data);
        //            Trace.WriteLine(data1);
        //            // Send the data byte by byte
        //            foreach (byte b in data1)
        //            {
        //                Trace.WriteLine(b);
        //                byte[] byteToSend = new byte[] { b };
        //                Trace.WriteLine(byteToSend);
        //                serialPort.Write(byteToSend, 0, 1); // Send a single byte
        //                Thread.Sleep(100);
        //                WriteData($"Sent data: {b}");// Pause briefly before sending the next byte
        //            }
        //            Dispatcher.Invoke(() =>
        //            {
        //                Trace.WriteLine(data1);
        //                WriteData($"Sent data: 1 + {data}");  // Log the sent data to the UI
        //            });
        //            Thread.Sleep(1000); // Delay between sending data
        //            //SerialPort_DataSending();
        //        }
        //        catch (Exception ex)
        //        {
        //            para.Inlines.Add("Failed to SEND" + data + "\n" + ex + "\n");
        //            mcFlowDoc.Blocks.Add(para);
        //            Commdata.Document = mcFlowDoc;
        //        }
        //    }


        //}
        //private void WriteData(string text)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    Trace.WriteLine(text);
        //    Commdata.AppendText(text + Environment.NewLine);
        //    // Assign the value of the plot to the RichTextBox.
        //    para.Inlines.Add(text);
        //    mcFlowDoc.Blocks.Add(para);
        //    Commdata.Document = mcFlowDoc;
        //}
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);

                // Convert byte array to ASCII string
                string data = Encoding.ASCII.GetString(buffer);
                int numericValue = BitConverter.ToInt32(buffer, 0);
                float floatValue = BitConverter.ToSingle(buffer, 0);
                // Read the data from the serial port asynchronously
                // string data = serialPort.ReadExisting(); // You can also use ReadLine() if data is line-based
                Dispatcher.Invoke(() =>
                {
                    // Update the UI with the received data
                    ReceivedDataTextBox.AppendText(data + Environment.NewLine);
                    //ReceivedDataTextBox.AppendText("Received Value: " + numericValue + Environment.NewLine);
                    //ReceivedDataTextBox.AppendText("Received Value: " + floatValue + Environment.NewLine);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }
        private void SerialPort_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the data from the serial port (in bytes)
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);

                // Example: First byte could be an identifier for numeric data or ASCII
                if (buffer[0] == 1) // Assuming identifier 1 means numeric data
                {
                    int numericValue = BitConverter.ToInt32(buffer, 1); // Skipping the first byte
                    Dispatcher.Invoke(() =>
                    {
                        ReceivedDataTextBox.AppendText("Numeric Value: " + numericValue + Environment.NewLine);
                    });
                }
                else if (buffer[0] == 2) // Assuming identifier 2 means ASCII data
                {
                    string asciiValue = Encoding.ASCII.GetString(buffer, 1, buffer.Length - 1); // Skip identifier byte
                    Dispatcher.Invoke(() =>
                    {
                        ReceivedDataTextBox.AppendText("ASCII Value: " + asciiValue + Environment.NewLine);
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }
        private void SerialPort_DataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the data from the serial port using ReadExisting for ASCII data
                string data = serialPort.ReadExisting();

                // Append the received data to the StringBuilder
                _sb.Append(data);

                // If the data includes new line, we display it and reset the StringBuilder
                if (data.Contains(Environment.NewLine))
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Display the received data in the TextBox
                        ReceivedDataTextBox.AppendText(_sb.ToString());
                        //_sb.Clear(); // Clear the StringBuilder for the next data chunk
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }

    }

}

