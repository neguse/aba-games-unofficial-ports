"""Verify PR48 upload identity separately from public-edge reachability.

Cloudflare version metadata is authenticated control-plane evidence, not proof
that a browser can fetch or play the site. Only the specific HTTP403/error1010
edge refusal is classified as blocked; all other verification failures fail CI.
"""
import hashlib
import json
from pathlib import Path
import uuid
import re
import urllib.error
import urllib.request


def verify_version(version, version_id, message):
    if version.get("id") != version_id:
        raise ValueError("Cloudflare returned a different version ID")
    annotations = version.get("annotations", {})
    if annotations.get("workers/tag") != "pr-48":
        raise ValueError("Cloudflare version has the wrong PR tag")
    if annotations.get("workers/message") != message:
        raise ValueError("Cloudflare version provenance does not match this run")


def dist_manifest(directory):
    root = Path(directory)
    result = []
    for path in sorted(root.rglob("*")):
        if path.is_symlink():
            raise ValueError("Preview assets must not contain symlinks")
        if path.is_file():
            data = path.read_bytes()
            result.append({"path": path.relative_to(root).as_posix(),
                           "size": len(data), "sha256": hashlib.sha256(data).hexdigest()})
    return json.dumps(result, sort_keys=True, separators=(",", ":")).encode()


def preview_identity(receipt):
    records = [json.loads(line) for line in receipt.splitlines() if line.strip()]
    uploads = [record for record in records if record.get("type") == "version-upload"]
    if len(uploads) != 1:
        raise ValueError("Expected exactly one Wrangler version-upload receipt")
    record = uploads[0]
    if record.get("version") != 1 or record.get("worker_name") != "aba-games-unofficial-ports":
        raise ValueError("Unexpected Wrangler upload receipt")
    version_id = record.get("version_id", "")
    if str(uuid.UUID(version_id)) != version_id:
        raise ValueError("Upload returned an invalid version UUID")
    url = record.get("preview_url", "")
    if not re.fullmatch(
        r"https://" + version_id[:8] + r"-aba-games-unofficial-ports\.[a-z0-9-]+\.workers\.dev",
        url,
    ):
        raise ValueError("Upload returned no matching immutable version preview URL")
    return version_id, url


def check_public_preview(url, provenance, opener=urllib.request.urlopen):
    """No retry, client impersonation, credentials, or security-setting changes."""
    stage = "provenance"
    try:
        with opener(url + "/pr-preview.json", timeout=30) as response:
            actual = json.load(response)
        if actual != provenance:
            raise ValueError("Public preview provenance does not match this run")
        stage = "index"
        with opener(url + "/", timeout=30) as response:
            if response.status != 200:
                raise ValueError("Public preview index did not return HTTP 200")
        return {"status": "passed", "provenance": "matched", "index": "HTTP 200"}
    except urllib.error.HTTPError as error:
        # Preserve bounded evidence but never publish arbitrary server responses.
        body = error.read(65536).decode("utf-8", errors="replace")
        if error.code == 403 and re.search(r"\berror(?:\s+code)?\s*:\s*1010\b", body, re.I):
            return {
                "status": "blocked", "stage": stage, "http_status": 403,
                "cloudflare_error": 1010,
                "reason": "Public edge refused the CI client; browser review required",
            }
        raise
