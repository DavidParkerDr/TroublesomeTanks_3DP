/*
  Controller Controller
 
  Sends the sensor information up to the game
  
 */

#include <EEPROM.h>
#include <Adafruit_NeoPixel.h>

#define PIN 7

// Parameter 1 = number of pixels in strip
// Parameter 2 = pin number (most are valid)
// Parameter 3 = pixel type flags, add together as needed:
//   NEO_KHZ800  800 KHz bitstream (most NeoPixel products w/WS2812 LEDs)
//   NEO_KHZ400  400 KHz (classic 'v1' (not v2) FLORA pixels, WS2811 drivers)
//   NEO_GRB     Pixels are wired for GRB bitstream (most NeoPixel products)
//   NEO_RGB     Pixels are wired for RGB bitstream (v1 FLORA pixels, not v2)
Adafruit_NeoPixel strip = Adafruit_NeoPixel(10, PIN, NEO_GRB + NEO_KHZ800);

//#define TEST_MODE 0
const int NUM_CONNECTION_TYPES = 17;
const int NUM_PORTS = 7;
const int NUM_SAMPLES = 1;
const int TOLERANCE = 10;
const int PORT_MAP[] = { 5, 4, 3, 2, 1, 0, 6 }; // Maps the port number to the actual order they appear on the box
int samples[NUM_PORTS * NUM_SAMPLES];
int values[NUM_PORTS];
const int resistances[] = {
  1021,  // nothing connected
  932,   // charge - unpressed
  181,   // charge - pressed
  608,   // right forward - unpressed
  7,     // right forward - pressed
  699,   // right backward - unpressed
  21,    // right backward - pressed
  785,   // left forward - unpressed
  45,    // left forward - pressed
  836,   // left backward - unpressed
  93,    // left backward - pressed
  959,   // gun fire - unpressed
  327,   // gun fire - pressed
  1002,  // gun right - unpressed
  511,   // gun right - pressed
  978,   // gun left - unpressed
  402    // gun left - pressed
};

byte connections[NUM_PORTS];
int maxSample = 0;
int totalSamples = NUM_PORTS * NUM_SAMPLES;

const int ID_ADDRESS = 0;
const int ID_LENGTH = 10;
char ID[ID_LENGTH];

// the setup routine runs once when you press reset:
void setup() {
  // start serial port at 9600 bps:
  Serial.begin(9600);

  strip.begin();
  strip.show();  // Initialize all pixels to 'off'
  initialiseAnimation();
  initialiseSamples();
  EEPROM.get(ID_ADDRESS, ID);
  while (!Serial) {
    ;  // wait for serial port to connect. Needed for Leonardo only
  }
}

void printAllSamples() {
  Serial.print("maxSample = ");
  Serial.println(maxSample);
  for (int sampleIndex = 0; sampleIndex < NUM_SAMPLES; sampleIndex++) {
    printSample(sampleIndex);
  }
}
void printSample(int sampleIndex) {
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    int index = calculateIndex(portIndex, sampleIndex);
    Serial.print("sample[");
    Serial.print(sampleIndex);
    Serial.print("] port[");
    Serial.print(portIndex);
    Serial.print("] = ");
    Serial.println(samples[index]);
  }
}
int calculateIndex(int port, int sample) {
  return sample * NUM_PORTS + port;
}
void initialiseSamples() {
  for (int i = 0; i < totalSamples; i++) {
    samples[i] = 0;
  }
}
const byte ports[] = { 0, 1, 2, 3, 4, 5, 6 };

// Sends the serial data for the ports
// Sends an eight bit value for each port
// Does not send the value 255 as this is a flag value
// Packet has format 255, 'D', vvvvv..., checksum
void sendData() {
  byte check = 0;
  // Send the packet start
  Serial.write(255);
  Serial.write('D');
  Serial.write(NUM_PORTS);
  //Serial.println("---------------------------------");
  for (byte portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    /* Serial.print(i);
    Serial.print(": ");
    Serial.print(analogRead(ports[i]));
    Serial.print(" - ");
    // Get the value and divide to make a single byte*/
    //  int data = analogRead(ports[i]) >> 2;
    //Serial.println(data);
    byte id = connections[PORT_MAP[portIndex]];
#if TEST_MODE == 1
    //   data = i;
#endif
    // Bit stuff, we never send 255 as that is a flag value
    //   if (data == 255 ) data = 254;
    Serial.write(id);
    //Serial.print("bit shift");
    // Serial.println(id);

    check = check + id;
  }
  Serial.write(check);
}

// the loop routine runs over and over again forever:
void loop() {
  collectSamples();

  if (Serial.available() > 0) {
    int command = Serial.read();
    if (command == 'R') {
      // Serial.println("fish");

      //printConnections();
      sendData();
    } else if (command == 'P') {
      receivePanel();
    } else if (command == 'T') {
      initialiseAnimation();
    } else if (command == 'I') {
      identifyDevice();
    } else if (command == 'U') {  // Update ID
      //while(Serial.available() < 11) { }
      GetCh();  // Read space
      for (int i = 0; i < ID_LENGTH; ++i) {
        ID[i] = GetCh();
      }
      EEPROM.put(ID_ADDRESS, ID);
    }
  }
}

void identifyDevice() {
  Serial.write("Tankontroller\n");
  for (int i = 0; i < ID_LENGTH; ++i) {
    Serial.write(ID[i]);
  }
  Serial.write("\n");
}

void shiftSamples() {
  int lastSet = totalSamples - NUM_PORTS;
  for (int index = 0; index < lastSet; index++) {
    int nextIndex = index + NUM_PORTS;
    samples[index] = samples[nextIndex];
  }
}
void collectSamples() {
  if (NUM_SAMPLES > 1) {
    shiftSamples();
  }
  int lastSet = totalSamples - NUM_PORTS;
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    int data = analogRead(ports[portIndex]);
    int sampleIndex = portIndex + lastSet;
    samples[sampleIndex] = data;
  }
  if (maxSample < NUM_SAMPLES) {
    maxSample++;
  }
  averageSamples();
  identifyValues();
  // sumConnections();
}
void sumConnections() {
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    if (connections[portIndex] != 0) {
      Serial.println(connections[portIndex]);
    }
  }
}
void averageSamples() {
  int start = NUM_SAMPLES - maxSample;
  for (int sampleIndex = start; sampleIndex < NUM_SAMPLES; sampleIndex++) {
    for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
      int index = calculateIndex(portIndex, sampleIndex);
      if (sampleIndex == start) {
        values[portIndex] = samples[index];
      } else {
        values[portIndex] = values[portIndex] + samples[index];
      }
    }
  }
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    values[portIndex] = values[portIndex] / maxSample;
  }
}
void printValues() {
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    Serial.print("value[");
    Serial.print(portIndex);
    Serial.print("] = ");
    Serial.println(values[portIndex]);
  }
}
void printConnections() {
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    Serial.print("connection[");
    Serial.print(portIndex);
    Serial.print("] = ");
    Serial.println(connections[portIndex]);
  }
}
void identifyValues() {
  for (int portIndex = 0; portIndex < NUM_PORTS; portIndex++) {
    int value = values[portIndex];
    byte connection = IdentifyConnection2(value);
    connections[portIndex] = connection;
  }
  //printConnections();
}

byte IdentifyConnection2(int value) {
  int differences[NUM_CONNECTION_TYPES];
  int lowestValue = 10000;
  byte lowestIndex = 0;
  for (byte i = 0; i < NUM_CONNECTION_TYPES; i++) {
    int difference = abs(resistances[i] - value);
    differences[i] = difference;
    if (difference < lowestValue) {
      lowestValue = difference;
      lowestIndex = i;
    }
  }
  return lowestIndex;
}

byte IdentifyConnection(int value) {

  //Serial.print("Port: ");
  //Serial.print(port);
  //Serial.print(" is connected to ");
  if (IsNearly(value, 1021, TOLERANCE)) {
    //Serial.println("nothing");
    return 0;
  } else if (IsNearly(value, 932, TOLERANCE)) {
    //Serial.println("charge - unpressed");
    return 1;
  } else if (IsNearly(value, 181, TOLERANCE)) {
    // Serial.println("charge - pressed");
    return 2;
  } else if (IsNearly(value, 608, TOLERANCE)) {
    //Serial.println("right forward - unpressed");
    return 3;
  } else if (IsNearly(value, 7, TOLERANCE)) {
    // Serial.println("right forward - pressed");
    return 4;
  } else if (IsNearly(value, 699, TOLERANCE)) {
    // Serial.println("right backward - unpressed");
    return 5;
  } else if (IsNearly(value, 21, TOLERANCE)) {
    // Serial.println("right backward - pressed");
    return 6;
  } else if (IsNearly(value, 785, TOLERANCE)) {
    //Serial.println("left forward - unpressed");
    return 7;
  } else if (IsNearly(value, 45, TOLERANCE)) {
    // Serial.println("left forward - pressed");
    return 8;
  } else if (IsNearly(value, 836, TOLERANCE)) {
    // Serial.println("left backward - unpressed");
    return 9;
  } else if (IsNearly(value, 93, TOLERANCE)) {
    // Serial.println("left backward - pressed");
    return 10;
  } else if (IsNearly(value, 959, TOLERANCE)) {
    // Serial.println("gun fire - unpressed");
    return 11;
  } else if (IsNearly(value, 327, TOLERANCE)) {
    // Serial.println("gun fire - pressed");
    return 12;
  } else if (IsNearly(value, 1002, TOLERANCE)) {
    // Serial.println("gun right - unpressed");
    return 13;
  } else if (IsNearly(value, 511, TOLERANCE)) {
    // Serial.println("gun right - pressed");
    return 14;
  } else if (IsNearly(value, 978, TOLERANCE)) {
    //  Serial.println("gun left - unpressed");
    return 15;
  } else if (IsNearly(value, 402, TOLERANCE)) {
    // Serial.println("gun left - pressed");
    return 16;
  }
}

bool IsNearly(int data, int target, int tolerance) {
  int lowerBound = target - tolerance;
  int upperBound = target + tolerance;
  if (data > lowerBound && data < upperBound) {
    return true;
  }
  return false;
}

byte GetCh() {
  int ch;
  do {
    ch = Serial.read();
  } while (ch < 0);

  return (byte)ch;
}

void receivePanel() {
  byte pixelCount = GetCh();
  byte check = 0;
  for (int i = 0; i < pixelCount; i++) {
    byte index = GetCh();
    byte r = GetCh();
    check += r;
    byte g = GetCh();
    check += g;
    byte b = GetCh();
    check += b;
    strip.setPixelColor(index, r, b, g);
  }

  byte receivedCheck = GetCh();

  if (check == receivedCheck) {
    strip.show();
  } else {
    //  colorFill(20,0,0);
  }
}

// Fill the dots witn one colour
void initialiseAnimation() {
  colorFill(20, 0, 0);
  delay(500);
  colorFill(20, 0, 20);
  delay(500);
  colorFill(20, 20, 0);
  delay(500);
  colorFill(0, 0, 0);
}

// Fill the dots witn one colour
void colorFill(byte r, byte b, byte g) {
  for (uint16_t i = 0; i < strip.numPixels(); i++) {
    strip.setPixelColor(i, r, b, g);
  }
  strip.show();
}
