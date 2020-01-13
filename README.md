# PCB-Visual-Inspection-on-Raspberry-Pi
[C#][UWP] PCB Visual Inspection Device based on Raspberry Pi 3B (Windows IoT)


# Idea:
My idea was born during my internship at LACROIX Electronics. It's a electronics manufacturer company which produces parts for Bosch, VALEO, etc. One of the biggest devices there was SMT Wave Soldering Machine which solders components like resistors, capacitors, chips or LEDs. At each step of soldering there were a minicomputers with normal and IR cameras which have been doing visual inspection of PCBs. The algorithm was checking if the components are not missing or to much shifted. Considering the experience I've gained there and my knowledge and intrest in programming, I proposed my BSc thesis to design, build and program simplier version of that device (without soldering) which performs a visual inspection of PCBs.

# Topics covered:
Image processing, image recognition, numerical methods, IoT (Internet of Things), Raspberry Pi, .NET API, object-oriented programming

# Project:
There were two different approches to make this application on Raspberry Pi. The first one was to make app for Linux and written in Python, The second one was to make app for Windows 10 IoT and written in one of supported programming languages in UWP (.NET Core). Before I proposed my BSc thesis I didn't know anything about .NET but I had been working on C projects so I have choosen C# because its structure and writing style is similiar to C but with more advanced features like object-oriented programming or asynchronus programming and I took the challange made by myself to learn C#. Windows 10 IoT doesn't support other API than UWP so here the choice was limited. Also IoT solutions are widly used in manufacturer companies like LACROIX and there is a demand for IoT solutions in places like that.


