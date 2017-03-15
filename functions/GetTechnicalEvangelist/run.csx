﻿#load "..\Shared\SharedData.csx"
using System.Net;
public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<technicalevangelist> inTable, TraceWriter log)
{

    // Return all entries in the ArticleHeader Table
    return req.CreateResponse(HttpStatusCode.OK, (from te in inTable select te).ToList());

}
