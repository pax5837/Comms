BLEService bleService("f033cf82-73e3-4d91-b022-cade9d63c3b8");
BLEFloatCharacteristic writeFloatCharacteristic("e50c42b5-d65e-46c9-9a40-c8d30c675760", BLEWriteWithoutResponse);
BLEStringCharacteristic readStringCharacteristic("c709b99a-6829-4e67-8ebb-462235418cb5", BLERead | BLENotify, 200);
BLEStringCharacteristic writeStringCharacteristic("d939184f-6caf-4b11-82d0-2ad0258874d5", BLEWriteWithoutResponse, 200);

void setupBle(){
    if (!BLE.begin()) {
      Serial.println("Starting BLE failed!");
      blinkBleStartFailed();
    }

    setupBleService();

    // start advertising
    BLE.advertise();
    delay(100);
    Serial.println("example service started");
}

void setupBleService(){
    BLE.setLocalName("Example Service");
    BLE.setAdvertisedService(bleService);

    bleService.addCharacteristic(writeFloatCharacteristic);
    bleService.addCharacteristic(readStringCharacteristic);
    bleService.addCharacteristic(writeStringCharacteristic);

    // add service
    BLE.addService(bleService);

    // set the initial value for the characeristic:
    writeFloatCharacteristic.writeValue(0);
    readStringCharacteristic.writeValue("Hello");
    writeStringCharacteristic.writeValue("");  
}