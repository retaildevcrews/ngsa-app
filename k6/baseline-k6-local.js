 // 1. init code: Load local files, import modules, declare lifecycle functions
import http from 'k6/http';
import { sleep, check } from 'k6';

// Options
export const options = {
    vus: 10,
    duration: '10s',

// Stages: ramping up/down VUs
//     stages: [
//         { duration: '30s', target: 20 },
//         { duration: '15s', target: 10 },
//         { duration: '10s', target: 0 },
//       ],
  };

// 2. setup code: Set up data for processing, share data among VUs
// is called at the beginning of the test, after the init stage but before the VU stage.
export function setup() {
  }

// 3. VU code: Run the test function, usually default
export default function () {

    let maxSleep=0;
    let res
    let baseURL='http://localhost:8080';

    //Request #1
    res=http.get(baseURL+'/version');
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'minLength'
    //LR script contains validator: 'maxLength'
    check(res, {
            "contentType":(r) => r.headers['Content-Type'].startsWith('text/plain'),
                    "minLength":(r) => r.body.length >= 5,
                    "maxLength":(r) => r.body.length <= 20,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #2
    res=http.get(baseURL+'/index.html');
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'contains'
    check(res, {
            "contentType":(r) => r.headers['Content-Type'].startsWith('text/html'),
                    "contains1":(r) =>
                r.body.includes('<title>Swagger UI</title>'),
                    "contains2":(r) =>
                r.body.includes('<div id="swagger-ui">'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #3
    res=http.get(baseURL+'/swagger.json');
    //LR script contains validator: 'jsonObject'
    //No supported validators exist for this request.
    sleep(Math.random() * maxSleep)

    //Request #4
    res=http.get(baseURL+'/healthz');
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'notContains'
    check(res, {
            "contentType":(r) => r.headers['Content-Type'].startsWith('text/plain'),
                    "notContains1":(r) =>
                !r.body.includes('fail'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #5
    res=http.get(baseURL+'/api/featured/movie');
    //LR script contains validator: 'jsonObject'
    //No supported validators exist for this request.
    sleep(Math.random() * maxSleep)

    //Request #6
    res=http.get(baseURL+'/healthz/ietf');
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "contentType":(r) => r.headers['Content-Type'].startsWith('application/health+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #7
    res=http.get(baseURL+'/api/actors');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 100,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #8
    res=http.get(baseURL+'/api/movies');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 100,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #9
    res=http.get(baseURL+'/api/genres');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 21,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #10
    res=http.get(baseURL+'/api/actors?pageNumber=2');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 100,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #11
    res=http.get(baseURL+'/api/movies?pageNumber=2');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 100,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #12
    res=http.get(baseURL+'/api/actors/nm0000031');
    //LR script contains validator: 'jsonObject'
    //No supported validators exist for this request.
    sleep(Math.random() * maxSleep)

    //Request #13
    res=http.get(baseURL+'/api/actors/nm0000173');
    //LR script contains validator: 'notContains'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "notContains1":(r) =>
                !r.body.includes('"deathYear":'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #14
    res=http.get(baseURL+'/api/movies/tt0133093');
    //LR script contains validator: 'jsonObject'
    //No supported validators exist for this request.
    sleep(Math.random() * maxSleep)

    //Request #15
    res=http.get(baseURL+'/api/movies?genre=Action');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 100,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #16
    res=http.get(baseURL+'/api/movies?year=1999');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 39,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #17
    res=http.get(baseURL+'/api/movies?rating=8.5');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 20,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #18
    res=http.get(baseURL+'/api/movies?actorId=nm0000206');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 45,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #19
    res=http.get(baseURL+'/api/movies?genre=action&year=2000&rating=7&q=glad&actorid=nm0000128');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 1,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #20
    res=http.get(baseURL+'/api/actors?q=nicole');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 5,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #21
    res=http.get(baseURL+'/api/movies?q=ring');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 7,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #22
    res=http.get(baseURL+'/api/actors?q=notfoundtest');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 0,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #23
    res=http.get(baseURL+'/api/movies?q=notfoundtest');
    //LR script contains validator: 'jsonArray'
    check(res, {
            "jsonArray":(r) =>
            r.json().length === 0,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #24
    res=http.get(baseURL+'/api/actors?q=123456789012345678901');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #25
    res=http.get(baseURL+'/api/actors?q=a');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #26
    res=http.get(baseURL+'/api/actors?q=123456789012345678901');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #27
    res=http.get(baseURL+'/api/actors?q=a');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #28
    res=http.get(baseURL+'/api/actors?pageSize=0');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #29
    res=http.get(baseURL+'/api/actors?pageSize=1001');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #30
    res=http.get(baseURL+'/api/actors?pageNumber=0');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #31
    res=http.get(baseURL+'/api/actors?pageNumber=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #32
    res=http.get(baseURL+'/api/actors?pageNumber=10001');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #33
    res=http.get(baseURL+'/api/actors?pageNumber=0&pageSize=-1&q=a');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #34
    res=http.get(baseURL+'/api/actors/ab12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #35
    res=http.get(baseURL+'/api/actors/tt12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #36
    res=http.get(baseURL+'/api/actors/nm123');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #37
    res=http.get(baseURL+'/api/actors/nmabcde');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #38
    res=http.get(baseURL+'/api/actors/nm123');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #39
    res=http.get(baseURL+'/api/actors/nm00000');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #40
    res=http.get(baseURL+'/api/movies?q=a');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #41
    res=http.get(baseURL+'/api/movies?q=123456789012345678901');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #42
    res=http.get(baseURL+'/api/movies?pageSize=0');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #43
    res=http.get(baseURL+'/api/movies?pageSize=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #44
    res=http.get(baseURL+'/api/movies?pageSize=1001');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #45
    res=http.get(baseURL+'/api/movies?pageNumber=0');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #46
    res=http.get(baseURL+'/api/movies?pageNumber=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #47
    res=http.get(baseURL+'/api/movies?pageNumber=10001');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #48
    res=http.get(baseURL+'/api/movies?year=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #49
    res=http.get(baseURL+'/api/movies?year=1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #50
    res=http.get(baseURL+'/api/movies?year=1873');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #51
    res=http.get(baseURL+'/api/movies?year=2026');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #52
    res=http.get(baseURL+'/api/movies?rating=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #53
    res=http.get(baseURL+'/api/movies?genre=ab');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #54
    res=http.get(baseURL+'/api/movies?genre=123456789012345678901');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #55
    res=http.get(baseURL+'/api/movies?actorId=nm123');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #56
    res=http.get(baseURL+'/api/movies?actorId=ab12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #57
    res=http.get(baseURL+'/api/movies?actorId=tt12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #58
    res=http.get(baseURL+'/api/movies?actorId=NM12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #59
    res=http.get(baseURL+'/api/movies?pageNumber=0&pageSize=-1&q=a&genre=ab&year=123&rating=-1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #60
    res=http.get(baseURL+'/api/movies/ab12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #61
    res=http.get(baseURL+'/api/movies/nm12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #62
    res=http.get(baseURL+'/api/movies/TT12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #63
    res=http.get(baseURL+'/api/movies/tt123');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #64
    res=http.get(baseURL+'/api/movies/ttabcde');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #65
    res=http.get(baseURL+'/api/movies/tt00000');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #66
    res=http.get(baseURL+'/api/actors?pageSize=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #67
    res=http.get(baseURL+'/api/actors?pageNumber=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #68
    res=http.get(baseURL+'/api/actors?pageNumber=foo&pageSize=foo&q=a');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #69
    res=http.get(baseURL+'/api/movies?pageSize=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #70
    res=http.get(baseURL+'/api/movies?pageNumber=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #71
    res=http.get(baseURL+'/api/movies?year=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #72
    res=http.get(baseURL+'/api/movies?rating=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #73
    res=http.get(baseURL+'/api/movies?pageNumber=foo&pageSize=foo&q=a&genre=ab&year=foo&rating=foo');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #74
    res=http.get(baseURL+'/api/actors?pageSize=10.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #75
    res=http.get(baseURL+'/api/actors?pageNumber=10.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #76
    res=http.get(baseURL+'/api/movies?pageSize=10.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #77
    res=http.get(baseURL+'/api/movies?pageNumber=10.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #78
    res=http.get(baseURL+'/api/movies?year=2020.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #79
    res=http.get(baseURL+'/api/movies?rating=10.1');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'contentType'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 400,
                    "contentType":(r) => r.headers['Content-Type'].startsWith('application/problem+json'),

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #80
    res=http.get(baseURL+'/api/actors/nm12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 404,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #81
    res=http.get(baseURL+'/api/movies/tt12345');
    //LR script contains validator: 'statusCode'
    //LR script contains validator: 'jsonObject'
    check(res, {
            "statusCode":(r) =>
            r.status === 404,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #82
    res=http.get(baseURL+'/api/actors/nm0000173/foo');
    //LR script contains validator: 'statusCode'
    check(res, {
            "statusCode":(r) =>
            r.status === 404,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

    //Request #83
    res=http.get(baseURL+'/api/movies/tt0133093/foo');
    //LR script contains validator: 'statusCode'
    check(res, {
            "statusCode":(r) =>
            r.status === 404,

            },
            {url: res.request.url}
        );
    sleep(Math.random() * maxSleep)

}

// 4. teardown code: Process result of setup code, stop test environment
// is called at the end of a test, after the VU stage
export function teardown(data) {

}
