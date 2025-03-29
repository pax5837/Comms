void processCommand(){
  if(writeStringCharacteristic.written()){
    String incoming = writeStringCharacteristic.value();
    Serial.write("received");
    readStringCharacteristic.writeValue("Received: " + incoming);      
  }
}
