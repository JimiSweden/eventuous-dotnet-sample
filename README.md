## EvenStore.
see 'README EventStore how to install run and getting started.md'


## Troubleshooting -
If you app doesnt start >> 

## Important note! if the project doesn't start
- ensure the EventsoreClient connection string is corect, and ES is running.
- ensure mongodb...

I had a problem with visual studio 2022, installed with dotnet 6.0
and the projects had dotnet 5.0
Solution was to edit this in the config files

They should look like this
C:\[...]\dotnet-sample\Bookings\Bookings.csproj AND C:\[...]\dotnet-sample\Bookings\Bookings.Payments.csproj
```
<PropertyGroup>
  <TargetFramework>net6.0</TargetFramework>
</PropertyGroup>
...	
	
```

## prereq: 
### mongodb. 
it is used from Eventous to store the projections. 
### EventStore
for storing the events. The source data.



## configs:

### eventstore client: 
ensure to have user and password in connectionstring unless it is disabled in ESDB (see esdb readme)

### mongodb client: 
no user/password needed by default


# run the roject 
Debug the Bookings project
> if you like to also enable Bookings.Payment in multiple startup projects


# Try it out via swagger.
swagger will be available at 
(see launchSettings.json under 'C:\[...]\dotnet-sample\Bookings\Properties\' and 'C:\[...]\dotnet-sample\Bookings.Payments\Properties\'

## Bookings.Payments
https://localhost:44359/swagger/index.html
http://localhost:51219/swagger/index.html

## Bookings
https://localhost:44352/swagger/index.html
http://localhost:51218/swagger/index.html

### Usage examples:
### booking: https://localhost:44352/booking/book
```json
{
  "bookingId": "123",
  "guestId": "jimi@lee",
  "roomId": "13",
  "checkInDate": "2022-02-23T13:00:00.000Z",
  "checkOutDate": "2022-02-24T11:00:00.000Z",
  "bookingPrice": 99,
  "prepaidAmount": 15,
  "currency": "EUR",
  "bookingDate": "2022-02-22T20:24:38.040Z"
}
```


```
curl -X 'POST' \
  'https://localhost:44352/booking/book' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "bookingId": "123",
  "guestId": "jimi@lee",
  "roomId": "13",
  "checkInDate": "2022-02-23T13:00:00.000Z",
  "checkOutDate": "2022-02-24T11:00:00.000Z",
  "bookingPrice": 99,
  "prepaidAmount": 15,
  "currency": "EUR",
  "bookingDate": "2022-02-22T20:24:38.040Z"
}'
``` 

### get booking : https://localhost:44352/bookings/123 
```
curl -X 'GET' \
  'https://localhost:44352/bookings/123' \
  -H 'accept: text/plain'
```

response
```json
{
  "guestId": "jimi@lee",
  "roomId": {
    "value": "13"
  },
  "period": {
    "checkIn": "2022-02-23",
    "checkOut": "2022-02-24"
  },
  "price": {
    "amount": 99,
    "currency": "EUR"
  },
  "outstanding": {
    "amount": 84,
    "currency": "EUR"
  },
  "paid": false,
  "paymentRecords": [],
  "id": {
    "value": "123"
  }
}
```
### recordPayment
using the booking id from above, and the outstanding amount to fully pay the booking.
```json
{
  "bookingId": "123",
  "paidAmount": 84,
  "currency": "EUR",
  "paymentId": "123456",
  "paidBy": "jimi@lee"
}
```

```
curl -X 'POST' \
  'https://localhost:44352/booking/recordPayment' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "bookingId": "123",
  "paidAmount": 84,
  "currency": "EUR",
  "paymentId": "123456",
  "paidBy": "jimi@lee"
}'
```

result: as you can see there are two events, PaymentRecorded and FullyPaid
```json
{
  "state": {
    "guestId": "jimi@lee",
    "roomId": {
      "value": "13"
    },
    "period": {
      "checkIn": "2022-02-23",
      "checkOut": "2022-02-24"
    },
    "price": {
      "amount": 99,
      "currency": "EUR"
    },
    "outstanding": {
      "amount": 0,
      "currency": "EUR"
    },
    "paid": true,
    "paymentRecords": [
      {
        "paymentId": "123456",
        "paidAmount": {
          "amount": 84,
          "currency": "EUR"
        }
      }
    ],
    "id": {
      "value": "123"
    }
  },
  "success": true,
  "changes": [
    {
      "event": {
        "bookingId": "123",
        "paidAmount": 84,
        "outstanding": 0,
        "currency": "EUR",
        "paymentId": "123456",
        "paidBy": "jimi@lee",
        "paidAt": "2022-02-22T21:48:09.3615456+01:00"
      },
      "eventType": "V1.PaymentRecorded"
    },
    {
      "event": {
        "bookingId": "123",
        "fullyPaidAt": "2022-02-22T21:48:09.3615456+01:00"
      },
      "eventType": "V1.FullyPaid"
    }
  ]
}
```

now get the booking again, and you can see the payment in it.
```json
{
  "guestId": "jimi@lee",
  "roomId": {
    "value": "13"
  },
  "period": {
    "checkIn": "2022-02-23",
    "checkOut": "2022-02-24"
  },
  "price": {
    "amount": 99,
    "currency": "EUR"
  },
  "outstanding": {
    "amount": 0,
    "currency": "EUR"
  },
  "paid": true,
  "paymentRecords": [
    {
      "paymentId": "123456",
      "paidAmount": {
        "amount": 84,
        "currency": "EUR"
      }
    }
  ],
  "id": {
    "value": "123"
  }
}
```

# Look inside EventStore 
https://127.0.0.1:2113/web/index.html#/
> Login: admin/changeit  (unless you changed it ;-) 

> Note: if you get a certificate error; import the node cert in chrome, to access UI
> chrome://settings/security?search=cert 
> chrome > settings > security > manage certificates > import. "C:\ESDB\certs\node1\node.crt"
> And restart Chrome, somtimes it works... 

> IF you allready imported it, and gets a certificate alert/popup with MS-tokens, just cancel the alert.

## in the StreamBrowser you can see the booking events . 
https://127.0.0.1:2113/web/index.html#/streams
https://127.0.0.1:2113/web/index.html#/streams/Booking-123 

> And you can drill down into the events

```
Event #	Name			Type				Created Date	
2		2@Booking-123	V1.FullyPaid		2022-02-22 21:48:09	JSON
1		1@Booking-123	V1.PaymentRecorded	2022-02-22 21:48:09	JSON
0		0@Booking-123	V1.RoomBooked		2022-02-22 21:33:51	JSON
```