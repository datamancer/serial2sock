======================
DEV TEST INSTRUCTIONS
======================

1. Install the com0com package found in the zip file
2. move the "Socket.exe" file program found in the root
   folder into "../Program Files (x86)/com0com
3. Run "../Program Files (x86)/com0com/setupc/"
4. Enter the following command:

	busynames COM?*

5. Create a named COMX COMY port pair not in the list w/

	install portName=COMX portName=COMY

   where X, Y are integer values not in the 
   busynames list
6. Exit setupc with "Quit"
7. Open MainPower.Com0com.Redirector.sln in Visual 
   Studio 2015
8. Run the solution **

=======
TESTING
=======

1. In MainPower.Com0com.Redirector, select the port that
   you have created. You will only see one of these ports
   in the main window -- please ensure that the 
   PortB value that you have selected is prefixed with 
   COM- and an integer value.

2. Once the program hangs (TODO) run the SerialTester
   program found in

	SerialTester>SerialTester>obj>Debug
	>SerialTester.exe

3. Select the COM port associated with the first com 
   port in the pair. If you don't know what you selected, 
   you can check in Device Manager under com0com. 

4. Write a message to it and hit SEND
5. You should see a message appear in the Console view 
   of MainPower.Com0com.Redirector.

---------------------------------------------------------

** If the program can't find the com0com.INF files, 
   move the INF files into the specified location

Destination:	C:/Users/<USER>/Source/Repos/<program>