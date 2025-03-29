#define RED_PIN 22     
#define BLUE_PIN 24     
#define GREEN_PIN 23

const int colorRed = 1;
const int colorGreen = 2;
const int colorBlue = 3;
const int colorWhite = 4;
// Error led patterns should always have 5 elements (avoid using red)
int errorPatternBleStartFailed[5] = { colorBlue, colorBlue, colorBlue, colorBlue, colorBlue };
// end led error patters

void switchOnBlueLed(){
  digitalWrite(BLUE_PIN, LOW); 
}

void switchOffBlueLed(){
  digitalWrite(BLUE_PIN, HIGH); 
}

void switchOnBlueLedOnly(){
  switchOnBlueLed();
  switchOffRedLed();
  switchOffGreenLed();
}

void switchOnGreenLed(){
  digitalWrite(GREEN_PIN, LOW); 
}

void switchOnGreenLedOnly(){
  switchOffBlueLed();
  switchOffRedLed();
  switchOnGreenLed();
}

void switchOffGreenLed(){
  digitalWrite(GREEN_PIN, HIGH); 
}

void switchOnRedLed(){
  digitalWrite(RED_PIN, LOW);
}

void switchOffRedLed(){
  digitalWrite(RED_PIN, HIGH);
}

void switchOnRedLedOnly(){
  switchOffBlueLed();
  switchOnRedLed();
  switchOffGreenLed();
}

void blinkGreenLed(int times){
  for (int i = 0; i < times; i++) {
    switchOnGreenLed();
    delay(500);
    switchOffGreenLed();
    delay(500);
  }
}

void switchOffAllLeds(){
  switchOffGreenLed();
  switchOffBlueLed();
  switchOffRedLed();
}

void switchOnAllLeds(){
  
  switchOnGreenLed();
  switchOnBlueLed();
  switchOnRedLed();
}

void blinkBleStartFailed(){
  while(true){
    blinkWithErrorPattern(errorPatternBleStartFailed);
  }  
}

void blinkWithErrorPattern(int colors[])
{
    for(int j = 0; j < 5; j++){
      switchOffAllLeds();
      delay(150);
      switchOnRedLedOnly();
      delay(150);
    }

    for(int i = 0; i < 5; i++){
      switchOffAllLeds();
      delay(750);
      switch(colors[i])
      {
          case colorRed:
            switchOnRedLedOnly();
            break;
          case colorGreen:
            switchOnGreenLedOnly();
            break;
          case colorBlue:
            switchOnBlueLedOnly();
            break;
          case colorWhite:
            switchOnAllLeds();
            break;  
          default:
            break;
      }
      delay(750);
    }

    delay(1000);
}
