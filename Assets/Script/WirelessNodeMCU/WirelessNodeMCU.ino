#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_ADXL345_U.h>

const char* ssid = "LogicLover";
const char* password = "IceBender";

IPAddress unityIP(10, 87, 8, 231);
unsigned int unityPort = 25666;

IPAddress ip(10, 87, 8, 232);
IPAddress gateway(10, 87, 8, 187);
IPAddress subnet(255, 255, 255, 0);

WiFiUDP Udp;
Adafruit_ADXL345_Unified accel = Adafruit_ADXL345_Unified(12345);

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
  Serial.println("Connected to IP: " + WiFi.localIP().toString());

  if (!accel.begin())
  {
    Serial.println("No ADXL345 detected. Check wiring!");
    while (1);
  }
  accel.setRange(ADXL345_RANGE_2_G);

  Udp.begin(unityPort);
}

void loop()
{
  sensors_event_t event;
  accel.getEvent(&event);

  float values[3] =
  {
    event.acceleration.x,
    event.acceleration.y,
    event.acceleration.z
  };

  byte data[12];
  memcpy(data, values, 12);

  Udp.beginPacket(unityIP, unityPort);
  Udp.write(data, 12);
  Udp.endPacket();

  delay(200);
}