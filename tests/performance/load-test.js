import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(99)<1500'], // 99% of requests must complete below 1.5s
    errors: ['rate<0.1'], // Error rate must be below 10%
  },
};

const BASE_URL = __ENV.BASE_URL || 'https://staging.scoutvision.com';

export default function () {
  // Test home page
  let response = http.get(`${BASE_URL}/`);
  check(response, {
    'home page status is 200': (r) => r.status === 200,
    'home page loads in <2s': (r) => r.timings.duration < 2000,
  }) || errorRate.add(1);

  sleep(1);

  // Test API health endpoint
  response = http.get(`${BASE_URL}/api/health`);
  check(response, {
    'health endpoint status is 200': (r) => r.status === 200,
    'health endpoint responds in <500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);

  // Test player search
  response = http.get(`${BASE_URL}/api/players/search?q=test`);
  check(response, {
    'search endpoint status is 200': (r) => r.status === 200,
    'search responds in <1s': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);

  sleep(2);

  // Test AI prediction endpoint
  response = http.get(`${BASE_URL}/ai/predict/talent/1`);
  check(response, {
    'AI prediction status is 200': (r) => r.status === 200,
    'AI prediction responds in <5s': (r) => r.timings.duration < 5000,
  }) || errorRate.add(1);

  sleep(3);
}

export function handleSummary(data) {
  return {
    'performance-report.html': htmlReport(data),
    'performance-summary.json': JSON.stringify(data),
  };
}

function htmlReport(data) {
  return `
<!DOCTYPE html>
<html>
<head>
    <title>ScoutVision Performance Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .metric { margin: 10px 0; padding: 10px; background: #f5f5f5; }
        .pass { color: green; }
        .fail { color: red; }
    </style>
</head>
<body>
    <h1>ScoutVision Performance Test Report</h1>
    <h2>Summary</h2>
    <div class="metric">
        <strong>Total Requests:</strong> ${data.metrics.http_reqs.count}
    </div>
    <div class="metric">
        <strong>Failed Requests:</strong> ${data.metrics.http_req_failed.count}
    </div>
    <div class="metric">
        <strong>Average Response Time:</strong> ${data.metrics.http_req_duration.avg.toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>95th Percentile:</strong> ${data.metrics.http_req_duration['p(95)'].toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>99th Percentile:</strong> ${data.metrics.http_req_duration['p(99)'].toFixed(2)}ms
    </div>
</body>
</html>`;
}