#include <ArduinoBLE.h>
#include "src/vesc/VescUart.h"

#define DEBUG false

unsigned long com_cycle_time = 50;
unsigned long last_cycle = 0;

void setup() {
  setupDebugSerial();
  switchOnRedLedOnly();
  setupBle(); 
  switchOnGreenLedOnly();
}

void loop() {   
  if(millis() > last_cycle + com_cycle_time) {

    last_cycle = millis();    

    BLEDevice central = BLE.central();
    if (central && central.connected()) {
      switchOnBlueLedOnly();
      processCommand();
    }
    else{
      switchOnGreenLedOnly();
    }
  }
}

void setupDebugSerial() {
  Serial.begin(9600);
  if (DEBUG) {
    while(!Serial);
  }
}
