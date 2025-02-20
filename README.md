# url-shortener

A simple URL shortening service.


### Endpoints:
  1. POST /shorten Accepts a long URL and returns a shortened URL. Only urls with a http scheme are accepted.
  2. GET /{shortUrl} Accepts a short URL and redirects to the original long URL.
  3. GET /stats/{shortUrl} endpoint to show how many times a URL has been accessed.



### Store
 - DB: MongoDB
 - Schema: 
	- ShortUrl: String
	- LongUrl: String
	- AccessCount: Number