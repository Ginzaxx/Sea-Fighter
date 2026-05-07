#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "LogicLover";
const char* password = "IceBender";

IPAddress unityIP(10, 87, 8, 231);
unsigned int unityPort = 25666;

IPAddress ip(10, 87, 8, 231);
IPAddress gateway(10, 87, 8, 187);
IPAddress subnet(255, 255, 255, 0);

WiFiUDP Udp;

void setup()
{
  Serial.begin(115200);
  Serial.println();

  WiFi.hostname("LogicLover-01");
  WiFi.config(ip, gateway, subnet);

  Serial.printf("Connecting to %s ", ssid);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.println("Connected. IP: " + WiFi.localIP().toString());

  Udp.begin(unityPort);
}

void loop()
{
  int joystickX = analogRead(A0);

  int direction = 0;
  if (joystickX < 400)      direction = -1; // LEFT
  else if (joystickX > 600) direction =  1; // RIGHT

  byte data[4];
  data[0] = (direction >> 24) & 0xFF;
  data[1] = (direction >> 16) & 0xFF;
  data[2] = (direction >> 8)  & 0xFF;
  data[3] =  direction        & 0xFF;

  Udp.beginPacket(unityIP, unityPort);
  Udp.write(data, 4);
  Udp.endPacket();

  delay(1000);
}