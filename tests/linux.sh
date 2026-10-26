#!/bin/sh
# The Linux test, run as root on a distribution with its ca-certificates package, with the folder of the published
# program as the argument. It adds one of the listed roots as a local anchor (so there is something of each kind to
# find: the distribution's emSign ECC Root CA - G3 and the machine's own anchor), then checks that scan sees them
# trusted, that revoke leaves nothing on the list trusted, and that restore brings the bundle back byte for byte.
set -eu

app=$1
patch="$app/emudhra-patch"
root="$app/certs/7DD78D5F4F13459A83DFF9ABBBA62EDBAF6F2D102BF257FD712F4D9F2746ED8D.pem"

if [ -f /etc/ca-certificates.conf ]; then
    anchor=/usr/local/share/ca-certificates/test-emsign-root-tls-g3.crt
elif [ -d /etc/pki/ca-trust/source/anchors ]; then
    anchor=/etc/pki/ca-trust/source/anchors/test-emsign-root-tls-g3.pem
elif [ -d /etc/ca-certificates/trust-source/anchors ]; then
    anchor=/etc/ca-certificates/trust-source/anchors/test-emsign-root-tls-g3.pem
else
    echo "FAIL: no anchor directory this test knows"; exit 1
fi

for file in /etc/ssl/certs/ca-certificates.crt /etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem /etc/pki/tls/certs/ca-bundle.crt; do
    if [ -f "$file" ]; then bundle=$file; break; fi
done
[ -n "${bundle:-}" ] || { echo "FAIL: no CA bundle"; exit 1; }

rebuild() {
    if command -v update-ca-certificates >/dev/null 2>&1; then update-ca-certificates >/dev/null; fi
    if command -v update-ca-trust >/dev/null 2>&1; then update-ca-trust extract; fi
}

expect() {
    want=$1; shift
    set +e; "$@"; code=$?; set -e
    if [ "$code" != "$want" ]; then echo "FAIL: '$*' exited $code, expected $want"; exit 1; fi
}

mkdir -p "$(dirname "$anchor")"
cp "$root" "$anchor"
rebuild
before=$(sha256sum "$bundle" | cut -d' ' -f1)

echo "=== scan: the anchor and the distribution's root are trusted"
expect 2 "$patch" scan

echo "=== revoke"
expect 0 "$patch" revoke --yes
[ ! -e "$anchor" ] || { echo "FAIL: $anchor is still there"; exit 1; }
[ "$(sha256sum "$bundle" | cut -d' ' -f1)" != "$before" ] || { echo "FAIL: $bundle did not change"; exit 1; }

echo "=== scan: nothing trusted"
expect 0 "$patch" scan

echo "=== restore"
expect 0 "$patch" restore
[ -e "$anchor" ] || { echo "FAIL: $anchor was not put back"; exit 1; }
[ "$(sha256sum "$bundle" | cut -d' ' -f1)" = "$before" ] || { echo "FAIL: $bundle is not what it was"; exit 1; }

echo "=== scan: trusted again"
expect 2 "$patch" scan

rm -f "$anchor"
rebuild
echo "PASS"
