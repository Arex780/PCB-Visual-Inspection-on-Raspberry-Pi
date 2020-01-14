# PCB-Visual-Inspection-on-Raspberry-Pi
[C#][UWP] PCB Visual Inspection Device based on Raspberry Pi 3B (Windows IoT)


# Idea:
My idea was born during my internship at LACROIX Electronics. It's a electronics manufacturer company which produces parts for Bosch, VALEO, etc. One of the biggest devices there was SMT Wave Soldering Machine which solders components like resistors, capacitors, chips or LEDs. At each step of soldering there were a minicomputers with normal and IR cameras which have been doing visual inspection of PCBs. The algorithm was checking if the components are not missing or to much shifted. Considering the experience I've gained there and my knowledge and intrest in programming, I proposed my BSc thesis to design, build and program simplier version of that device (without soldering) which performs a visual inspection of PCBs.

# Topics covered:
Image processing, image recognition, numerical methods, IoT (Internet of Things), Raspberry Pi, .NET API, object-oriented programming

# Project:
There were two different approches to make this application on Raspberry Pi. The first one was to make app for Linux and written in Python, The second one was to make app for Windows 10 IoT and written in one of supported programming languages in UWP (.NET Core). Before I proposed my BSc thesis I didn't know anything about .NET but I had been working on C projects so I have choosen C# because its structure and writing style is similiar to C but with more advanced features like object-oriented programming or asynchronus programming and I took the challange made by myself to learn C#. Windows 10 IoT doesn't support other API than UWP so here the choice was limited. Also IoT solutions are widely used in manufacturer companies like LACROIX and there is a demand for IoT solutions in places like that.

# Functions:
to be added

# Structure:
The project contains of 3 pages and 3 classes. On MainPage there are checkboxes to choose which algorithm will be executed, camera preview and buttons (Start test, Save PCB3, Exit). From this page the user can navigate to Settings Page (SecondPage) or to Report Page (ThirdPage). On SecondPage the user can choose which PCB will be tested and there are many comboboxes&checkboxes with settings for each algorithm and with parameters of testing method. There is also a hyperlink button that will navigate to Camera Preview (MainPage). On ThirdPage there are original and processed images of reference and tested board that can be zoomed in and also there are detailed information good/bad pixel count and overall score. From this page the user can navigate to Camera Preview (MainPage). As the name suggets CameraControl class is used to initialize camera preview and take pictures. In the ImageProcessing class there are methods that transform image to arrays, transform arrays to image, compare pictures, check background, create heatmap & scoremap and save image to bitmap file. In the EdgeDetection class there are matrixes for 3 operators (Sobel, Perwitt and Kirsch) and method that performs convolution filtering to obtain edges from the picture.

# Summary:
to be added

# Possible improvments:
to be added
