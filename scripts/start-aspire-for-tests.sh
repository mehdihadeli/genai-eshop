#!/usr/bin/env bash
set -euo pipefail

base_urls=(
  "${GENAI_TEST_CATALOGS_BASE_URL:-http://localhost:5000}"
  "${GENAI_TEST_CARTS_BASE_URL:-http://localhost:4000}"
  "${GENAI_TEST_ORDERS_BASE_URL:-http://localhost:7000}"
  "${GENAI_TEST_REVIEWS_BASE_URL:-http://localhost:8000}"
  "${GENAI_TEST_RECOMMENDATION_BASE_URL:-http://localhost:2000}"
)

start_log="${TMPDIR:-/tmp}/genai-eshop-aspire-start.json"

aspire start \
  --format json \
  --project src/Aspire/Aspire.AppHost/Aspire.AppHost.csproj \
  > "$start_log"

for attempt in {1..60}; do
  ready=true
  for base_url in "${base_urls[@]}"; do
    if ! curl --fail --silent --show-error "$base_url/health" > /dev/null; then
      ready=false
      break
    fi
  done

  if [[ "$ready" == true ]]; then
    echo "Aspire services are ready. Run tests from Rider now."
    exit 0
  fi

  if [[ "$attempt" == 60 ]]; then
    echo "Timed out waiting for Aspire services." >&2
    cat "$start_log" >&2
    exit 1
  fi

  sleep 5
done
