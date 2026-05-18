import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 10 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1000'],
  },
};

const baseUrl = __ENV.BASE_URL || 'http://api:9005';

export default function () {
  const response = http.get(`${baseUrl}/health`);

  check(response, {
    'health status is ok': (res) => res.status >= 200 && res.status < 500,
  });

  sleep(1);
}
