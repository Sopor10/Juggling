#!/usr/bin/env node
import { chmodSync, existsSync, mkdirSync, readFileSync, renameSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { homedir } from "node:os";

const HOST_AUTH_FILE = process.env.HOST_AUTH_FILE || "/host-hermes-auth.json";
const PI_AUTH_FILE = process.env.PI_AUTH_FILE || join(homedir(), ".pi", "agent", "auth.json");
const force = process.argv.includes("--force");

function fail(message) {
  console.error(`Pi Codex auth: ${message}`);
  process.exit(1);
}

function readJson(path, label) {
  try {
    return JSON.parse(readFileSync(path, "utf8"));
  } catch (error) {
    fail(`cannot read ${label} at ${path}: ${error.message}`);
  }
}

function decodeJwtPayload(token) {
  try {
    const [, payload] = token.split(".");
    if (!payload) return undefined;
    return JSON.parse(Buffer.from(payload, "base64url").toString("utf8"));
  } catch {
    return undefined;
  }
}

function stringValue(...values) {
  return values.find((value) => typeof value === "string" && value.length > 0);
}

function readExistingAuth() {
  if (!existsSync(PI_AUTH_FILE)) return {};
  const value = readJson(PI_AUTH_FILE, "Pi auth");
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    fail(`Pi auth at ${PI_AUTH_FILE} must contain a JSON object`);
  }
  return value;
}

if (!existsSync(PI_AUTH_FILE) && !existsSync(HOST_AUTH_FILE)) {
  fail(
    `no host OpenAI Codex OAuth login found. Expected ${HOST_AUTH_FILE}; log in with Hermes on the host first, then recreate the container`,
  );
}

const existing = readExistingAuth();
if (!force && existing["openai-codex"]?.type === "oauth") {
  // The Pi auth volume is writable and persists refreshed tokens. Do not replace
  // a refreshed token with the read-only host snapshot on every invocation.
  process.exit(0);
}

if (!existsSync(HOST_AUTH_FILE)) {
  fail(`host Hermes auth is missing at ${HOST_AUTH_FILE}`);
}

const hostAuth = readJson(HOST_AUTH_FILE, "host Hermes auth");
const hermesCredentials = hostAuth?.credential_pool?.["openai-codex"];
const hostCredential = Array.isArray(hermesCredentials)
  ? [...hermesCredentials].sort((a, b) => (a?.priority ?? 999) - (b?.priority ?? 999))[0]
  : undefined;
const tokens = hostCredential?.access_token
  ? hostCredential
  : hostAuth?.tokens;
if (!tokens || typeof tokens !== "object") {
  fail("host Hermes auth has no openai-codex OAuth credential; an interactive/API-key login is not supported");
}

const access = stringValue(tokens.access_token, tokens.access);
const refresh = stringValue(tokens.refresh_token, tokens.refresh);
const accessPayload = access ? decodeJwtPayload(access) : undefined;
const accountId = stringValue(
  tokens.account_id,
  tokens.accountId,
  accessPayload?.["https://api.openai.com/auth"]?.chatgpt_account_id,
);
if (!access || !refresh || !accountId) {
  fail("host Hermes auth is missing access token, refresh token, or account id");
}

const exp = Number(accessPayload?.exp);
const expires = Number.isFinite(exp) && exp > 0 ? exp * 1000 : Date.now() + 5 * 60 * 1000;
const next = {
  ...existing,
  "openai-codex": {
    type: "oauth",
    access,
    refresh,
    expires,
    accountId,
  },
};

mkdirSync(dirname(PI_AUTH_FILE), { recursive: true, mode: 0o700 });
const temporary = `${PI_AUTH_FILE}.tmp-${process.pid}`;
writeFileSync(temporary, `${JSON.stringify(next, null, 2)}\n`, { encoding: "utf8", mode: 0o600 });
renameSync(temporary, PI_AUTH_FILE);
try {
  // chmod also fixes permissions when the file was created by a pre-existing volume.
  chmodSync(PI_AUTH_FILE, 0o600);
} catch {
  // The write already succeeded; permission errors are reported by Pi if relevant.
}
console.error(`Pi Codex auth: configured ${PI_AUTH_FILE} from the host login`);
