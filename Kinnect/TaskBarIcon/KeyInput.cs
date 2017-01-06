﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TaskBarIcon;

namespace FileManager
{
    class KeyInput
    {

        private XDocument keyCommandDoc = null;
        private XmlDocument kinnectXMLCommands;
        private String xmlKeyCommandFileName = "KeyCommands.xml"; //this xml document contains all data about saved movements.
        private String xmlKinectMovementFileName = "KinectCommands.xml"; //this xml document contains windows key combination commands.
        private int id;


        //Constructor
        public KeyInput()
        {


            try
            {
                keyCommandDoc = XDocument.Load(xmlKeyCommandFileName);
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine(xmlKeyCommandFileName + " not found");
            }
            fileExist("/" + xmlKinectMovementFileName, "movements");
            kinnectXMLCommands = new XmlDocument();
            kinnectXMLCommands.Load("/" + xmlKinectMovementFileName);


        }


        //Write a command into the xml file
        public void writeCommand(Commands.Command command)
        {
            XmlElement movement = kinnectXMLCommands.CreateElement("command");
            XmlElement id = kinnectXMLCommands.CreateElement("id");
            id.InnerText = command.keyID.ToString();
            XmlElement totalTime = kinnectXMLCommands.CreateElement("total_time");
            totalTime.InnerText = command.totalTime.ToString();
            XmlElement moments = kinnectXMLCommands.CreateElement("moments");
            foreach (Commands.MomentInTime moment in command.points)
            {
                XmlElement cmoment = kinnectXMLCommands.CreateElement("moment");
                XmlElement time = kinnectXMLCommands.CreateElement("time");
                time.InnerText = moment.time.ToString();

                XmlElement points = kinnectXMLCommands.CreateElement("points");
                foreach (KeyValuePair<int, CameraSpacePoint> pair in moment.hand)
                {
                    XmlElement point = kinnectXMLCommands.CreateElement("point");
                    XmlElement key = kinnectXMLCommands.CreateElement("key");
                    XmlElement x = kinnectXMLCommands.CreateElement("x");
                    XmlElement y = kinnectXMLCommands.CreateElement("y");
                    XmlElement z = kinnectXMLCommands.CreateElement("z");
                    key.InnerText = pair.Key.ToString();
                    x.InnerText = pair.Value.X.ToString();
                    y.InnerText = pair.Value.Y.ToString();
                    z.InnerText = pair.Value.Z.ToString();
                    point.AppendChild(key);
                    point.AppendChild(x);
                    point.AppendChild(y);
                    point.AppendChild(z);
                    points.AppendChild(point);
                }
                cmoment.AppendChild(time);
                cmoment.AppendChild(points);
                moments.AppendChild(cmoment);
            }
            movement.AppendChild(id);
            movement.AppendChild(totalTime);
            movement.AppendChild(moments);
            kinnectXMLCommands.DocumentElement.AppendChild(movement);
            kinnectXMLCommands.Save("/" +  xmlKinectMovementFileName);
        }

        //Read a command from the xml file
        public Commands.Command readCommand(int keyInputID) 
        {
            Commands.Command command = new Commands.Command();
            XmlNodeList movements = kinnectXMLCommands.GetElementsByTagName("command");
            if (movements.Count > 0)
            {
                foreach (XmlNode movement in movements)
                {
                    if( Int32.Parse(movement.SelectSingleNode("id").InnerText) == keyInputID)
                    {
                        
                        break;
                        return command;
                    }
                }
            }
            command.keyID = -1;
            return command;
        }

        //Read all commands from the xml file
        public void readAllCommands()
        {
        }

        //check the file (filename) existence
        //and if the file does not exist, create the file
        private void fileExist(String filename, String startingTag)
        {
            if (!File.Exists(filename))
            {
                File.Create(filename);
                XDocument doc = new XDocument(filename, new XElement(startingTag));
            }

        }


        //Based on the ID this method send a key command for the windows
        public void sendKey(int ID)
        {
            //searching command 
            foreach (XElement xe in keyCommandDoc.Descendants("Item"))
            {
                if (xe.Element("ID").Value.Equals(ID + ""))
                {
                    //sending the key input
                    System.Windows.Forms.SendKeys.SendWait(xe.Element("Name").Value);
                    break;
                }
            }
        }
    }
}
