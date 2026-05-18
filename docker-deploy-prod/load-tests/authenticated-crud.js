import http from 'k6/http';
import { check, sleep } from 'k6';

const profile = __ENV.K6_PROFILE || 'smoke';

export const options = profile === 'stress'
  ? {
      stages: [
        { duration: __ENV.K6_RAMP_UP || '30s', target: Number(__ENV.K6_TARGET || 5) },
        { duration: __ENV.K6_HOLD || '1m', target: Number(__ENV.K6_TARGET || 10) },
        { duration: __ENV.K6_RAMP_DOWN || '30s', target: 0 },
      ],
      thresholds: {
        http_req_failed: [`rate<${__ENV.K6_FAILED_RATE || '0.05'}`],
        http_req_duration: [`p(95)<${__ENV.K6_P95_MS || '1500'}`],
      },
    }
  : {
      vus: Number(__ENV.K6_VUS || 1),
      iterations: Number(__ENV.K6_ITERATIONS || 5),
      thresholds: {
        http_req_failed: [`rate<${__ENV.K6_FAILED_RATE || '0.05'}`],
        http_req_duration: [`p(95)<${__ENV.K6_P95_MS || '3000'}`],
      },
    };

const baseUrl = __ENV.BASE_URL || 'http://api:9005';
const authUrl = __ENV.AUTH_URL || 'http://keycloak:8080';
const realm = __ENV.AUTH_REALM || 'productservice';
const clientId = __ENV.AUTH_CLIENT_ID || 'productservice-dev-blazor';
const username = __ENV.AUTH_USERNAME || 'admin';
const password = __ENV.AUTH_PASSWORD || 'admin123';

function token() {
  const response = http.post(
    `${authUrl}/realms/${realm}/protocol/openid-connect/token`,
    {
      client_id: clientId,
      grant_type: 'password',
      username,
      password,
    },
    { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } },
  );

  check(response, { 'token acquired': (res) => res.status === 200 });
  return response.json('access_token');
}

function jsonHeaders(accessToken) {
  return {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
  };
}

function responseId(response) {
  if (response.status < 200 || response.status >= 300 || !response.body) {
    return undefined;
  }

  try {
    const body = response.json();
    return body?.data?.id || body?.id || body?.data?.Id || body?.Id;
  } catch {
    return undefined;
  }
}

export default function () {
  const accessToken = token();
  const headers = jsonHeaders(accessToken);
  const suffix = `${__VU}-${__ITER}-${Date.now()}`;

  const productPayload = JSON.stringify({
    name: `K6 Product ${suffix}`,
    price: 19.9,
    type: 1,
    isActive: true,
  });

  const createProduct = http.post(`${baseUrl}/api/v1/Product`, productPayload, headers);
  check(createProduct, { 'product created': (res) => res.status >= 200 && res.status < 300 });

  const productId = responseId(createProduct);
  if (!productId) {
    return;
  }

  check(http.get(`${baseUrl}/api/v1/Product/${productId}`, headers), {
    'product read': (res) => res.status === 200,
  });

  const updateProduct = http.put(
    `${baseUrl}/api/v1/Product/${productId}`,
    JSON.stringify({
      id: productId,
      name: `K6 Product Updated ${suffix}`,
      price: 21.5,
      type: 1,
      isActive: true,
    }),
    headers,
  );
  check(updateProduct, { 'product updated': (res) => res.status >= 200 && res.status < 300 });

  const deleteProductPayload = JSON.stringify({
    name: `K6 Disposable Product ${suffix}`,
    price: 9.9,
    type: 2,
    isActive: true,
  });

  const createDisposableProduct = http.post(`${baseUrl}/api/v1/Product`, deleteProductPayload, headers);
  check(createDisposableProduct, {
    'disposable product created': (res) => res.status >= 200 && res.status < 300,
  });

  const disposableProductId = responseId(createDisposableProduct);
  if (disposableProductId) {
    check(http.del(`${baseUrl}/api/v1/Product/${disposableProductId}`, null, headers), {
      'product deleted': (res) => res.status >= 200 && res.status < 300,
    });
  }

  const orderPayload = JSON.stringify({
    products: [{ productId, quantity: 1 }],
    isActive: true,
  });
  const createOrder = http.post(`${baseUrl}/api/v1/Orders`, orderPayload, headers);
  check(createOrder, { 'order created': (res) => res.status >= 200 && res.status < 300 });

  const orderId = responseId(createOrder);
  if (orderId) {
    check(http.get(`${baseUrl}/api/v1/Orders/${orderId}`, headers), {
      'order read': (res) => res.status === 200,
    });

    check(http.del(`${baseUrl}/api/v1/Orders/${orderId}`, null, headers), {
      'order deleted': (res) => res.status >= 200 && res.status < 300,
    });
  }

  sleep(1);
}
