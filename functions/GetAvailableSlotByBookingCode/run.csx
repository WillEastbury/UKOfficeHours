﻿#load "..\Shared\httpUtils.csx"
#load "..\Domain\BookingSlot.csx"
#load "..\Domain\isv.csx"

#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Configuration;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json; 

public static HttpResponseMessage Run(HttpRequestMessage req,
     IQueryable<bookingslot> bookingSlotTable,
     IQueryable<isv> partnerTable,
     TraceWriter log)
{
    string bookingCode = "";
    try
    {
        bookingCode = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "BookingCode", true) == 0).Value;
    }
    catch (System.Exception)
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Booking Code missing!");
    }

    string topic = partnerTable.Where(row => row.CurrentCode == bookingCode).Select(items => items.ContactTopic).FirstOrDefault();
    if (topic == null)
    {
        return req.CreateResponse(HttpStatusCode.NotFound, "Booking Code not found!");
    }

    log.Info($"Booking Code:{bookingCode} has been reserved for '{topic}'.");

    DateTime outdate = DateTime.Now.AddDays(90);
    IOrderedEnumerable<bookingslot> bookingSlots;

    if (httpUtils.IsAuthenticated())
    {
        // Logged in employees can book anything not already booked
        bookingSlots = (from slot in bookingSlotTable select slot)
            .Where(e => 

                        e.StartDateTime >= DateTime.Now &&
                        e.StartDateTime <= outdate &&
                        e.BookedToISV == "None" &&
                        e.Topic == topic)
            .ToList()
            .OrderBy(e => e.StartDateTime);
    }
    else
    {
        // Don't show ADS' to customers to book directly 
        bookingSlots = (from slot in bookingSlotTable select slot)
            .Where(e => 
                        e.Duration < 120 && 
                        e.StartDateTime >= DateTime.Now &&
                        e.StartDateTime <= outdate &&
                        e.BookedToISV == "None" &&
                        e.Topic == topic)
            .ToList()
            .OrderBy(e => e.StartDateTime);
    }

    log.Info(bookingSlots.Count().ToString());
        
    // Return all entries in the BookingSlot Table
    return req.CreateResponse(HttpStatusCode.OK, bookingSlots);
}
