# Log2Twitter
Simple base for posting updates to twitter. Original purpose is to post important
information to private twitter clients to make sure servers are healthy.

So to simplyfy it, this is my version of public logging from servers that run
stuff that other people are interested in works as it should, and that they can
get fast friendly updates from if something goes kaboom.

## Setup
You need to create a Twitter-App that will be the poster of your information. You can use
same Twitter-App for how many clients as you like, but the app is not something i share, so
just go make your own =)

## Auth.
I also included a simple oauth-flow to get them client keys. That's what the Api is for. It is
vital that the address to your api is the same as you defined in your Twitter-App under allowed
callbackUrls, or you will get an exception.

The address does not need to be "real", but it needs to look legit. Since it isn't anything i made
public, I for instance use a local address (dev.log2twitter.local), and it goes fine as long as you
set it up with your iis.

So point IIS to Api-path and hit it with /oauth  and servicestack should help you with rest. If
callbackUrl is ok, you should get a success response with a redirectlink, click it and authorize
the app, then you get back and will see your client-keys.

## Other
Console-App is for testing to post.

