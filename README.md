# POSales
a c# desktopinventory management and point of sale app
# The configuration screen is fully compatible with IP addresses, named instances, and custom network setups!

When you open the Database Settings dialog, you can configure these options as follows:

1. Connecting to another PC via IP Address (e.g., over local network/Wi-Fi)
Server Name: 192.168.0.153 (or 192.168.0.153\SQLEXPRESS if it is a named SQL Server Express instance)
Database: MdemyPOS
Encrypt Connection: Check this if you want encrypted traffic, or uncheck if encryption is not configured.
Trust Server Certificate: Check this box (set to True) to prevent connection failure if the target SQL Server does not have a trusted SSL certificate installed.
2. Connecting to a local named SQL Express instance
Server Name: DESKTOP-C4UJ6TV\SQLEXPRESS
Database: MdemyPOS
3. Connecting to your own local PC
Server Name: OMATSULI-TOJU-J (or simply . or (local))
Database: MdemyPOS
How to test:
Open the Database Settings panel from the login screen.
Enter the host PC's IP address (e.g., 192.168.0.153) or name in the Server Name textbox.
Check the Trust Server Certificate box.
Click the Test Connection button. The application will attempt to connect immediately and alert you if the test succeeds or fails.
Once successful, click Save to persist the new configuration.
