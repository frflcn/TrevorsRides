# Notes about the code:

There are four projects:
- The Rider App
- The Driver App
- The Server
- The Helper Project

# The Server
The brain of the server is the RideMatchingService class. This is passed as a singleton to the DriverController and RiderController. The Driver and Rider controllers are where the driver and rider connect via websockets respectively. The RideMatchingService is responsible for maintaining connections to the driver and rider, and matching them when a ride request is made. The RideMatchingService keeps track of the riders and drivers by maintaining two dictionairies, one for the riders and one for the drivers. When a driver or rider logs in or logs out, the controller needs to add or remove the user from these dictionairies. To facilitate communication between the riders and drivers, the controllers will add a Send delegate to the driver and rider objects added to the dictionaires, which the RideMatchingService can use to send messages. To be able to listen to responses from the driver or rider the RideMatchingService needs to register a TaskCompletionSource with the controller which is stored in a dictionary in the controller with the key being the message id of the message sent, which is returned in the response. The RideMatchingService stores information on the progress of trips in the RidesModel database.

# The Rider and Driver App
To understand how the Rider and Driver apps communicate with the server you can look at the RideRequestService partial classes in each of the apps. This is where the apps connect to the server.
