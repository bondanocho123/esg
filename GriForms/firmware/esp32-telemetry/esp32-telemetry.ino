// ESP32 + DHT11: kirim suhu dan kelembapan ke GriForms (POST /api/telemetry) tiap 5 detik.
//
// Library (Arduino Library Manager): "DHT sensor library" (Adafruit) dan "ArduinoJson" v7.
// Salin secrets.example.h menjadi secrets.h lalu isi WiFi, URL server, dan API key dari halaman /telemetry.
//
// Jaringan saat development:
// - Jalankan server di semua interface: dotnet run --project src/GriForms.Web --launch-profile http --urls http://0.0.0.0:5158
// - SERVER_URL memakai IP laptop (ipconfig), bukan localhost. Izinkan port 5158 di Windows Firewall (jaringan Private).
// - ESP32 hanya mendukung WiFi 2,4 GHz, dan harus satu jaringan dengan laptop.

#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <DHT.h>
#include <time.h>
#include "secrets.h"

#define DHT_PIN 4
#define DHT_TYPE DHT11

const unsigned long SEND_INTERVAL_MS = 5UL * 1000;   // server membatasi 20 request/menit per device
const unsigned long WIFI_TIMEOUT_MS = 15UL * 1000;
const time_t MIN_VALID_EPOCH = 1700000000;   // jam di bawah ini berarti NTP belum sinkron

DHT dht(DHT_PIN, DHT_TYPE);
unsigned long lastSend = 0;
bool sentOnce = false;

void connectWiFi() {
  if (WiFi.status() == WL_CONNECTED) return;

  Serial.print("Menghubungkan WiFi");
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  unsigned long start = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - start < WIFI_TIMEOUT_MS) {
    delay(500);
    Serial.print('.');
  }

  if (WiFi.status() == WL_CONNECTED) {
    Serial.print(" terhubung, IP ");
    Serial.println(WiFi.localIP());
  } else {
    Serial.println(" gagal, dicoba lagi pada pengiriman berikutnya");
  }
}

bool clockSynced() {
  return time(nullptr) > MIN_VALID_EPOCH;
}

String isoUtcNow() {
  time_t now = time(nullptr);
  struct tm t;
  gmtime_r(&now, &t);
  char buf[25];
  strftime(buf, sizeof buf, "%Y-%m-%dT%H:%M:%SZ", &t);
  return String(buf);
}

void addReading(JsonArray readings, const char* metric, float value, const char* unit) {
  JsonObject r = readings.add<JsonObject>();
  r["metric"] = metric;
  r["value"] = roundf(value * 10) / 10;
  r["unit"] = unit;
}

void sendReadings(float temperature, float humidity) {
  JsonDocument doc;
  // Tanpa NTP, recordedAt tidak dikirim dan server memakai jam terima
  if (clockSynced()) doc["recordedAt"] = isoUtcNow();
  JsonArray readings = doc["readings"].to<JsonArray>();
  addReading(readings, "temperature", temperature, "C");
  addReading(readings, "humidity", humidity, "%");

  String body;
  serializeJson(doc, body);

  HTTPClient http;
  http.begin(SERVER_URL);
  http.setTimeout(10000);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("X-Device-Key", DEVICE_API_KEY);

  int code = http.POST(body);
  if (code == 202) {
    Serial.printf("Terkirim: %.1f C, %.0f %%\n", temperature, humidity);
  } else {
    String detail = code > 0 ? http.getString() : http.errorToString(code);
    Serial.printf("Gagal kirim (HTTP %d): %s\n", code, detail.c_str());
  }
  http.end();
}

void setup() {
  Serial.begin(115200);
  dht.begin();

  WiFi.setAutoReconnect(true);
  connectWiFi();
  configTime(0, 0, "pool.ntp.org", "time.google.com");   // UTC
}

void loop() {
  if (sentOnce && millis() - lastSend < SEND_INTERVAL_MS) {
    delay(100);
    return;
  }
  lastSend = millis();
  sentOnce = true;

  float humidity = dht.readHumidity();
  float temperature = dht.readTemperature();
  if (isnan(humidity) || isnan(temperature)) {
    Serial.println("Gagal membaca DHT11, periksa kabel dan pin data");
    return;
  }

  connectWiFi();
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi putus, pengiriman ditunda");
    return;
  }

  sendReadings(temperature, humidity);
}
