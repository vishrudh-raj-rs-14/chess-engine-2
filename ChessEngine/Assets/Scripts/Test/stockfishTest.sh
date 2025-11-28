#!/bin/bash

if [ $# -ne 2 ]; then
  echo "Usage: $0 <fen_json_file> <max_depth>"
  exit 1
fi

FEN_FILE=$1
MAX_DEPTH=$2
OUTPUT="perft_results.json"

echo "[" > "$OUTPUT"
FIRST_FEN=true

# UPDATED: read from .fens[]
jq -r '.fens[]' "$FEN_FILE" | while IFS= read -r FEN; do

  # JSON formatting for multiple FEN entries
  if [ "$FIRST_FEN" = true ]; then
    FIRST_FEN=false
  else
    echo "," >> "$OUTPUT"
  fi

  echo "  {" >> "$OUTPUT"
  echo "    \"fen\": \"${FEN}\"," >> "$OUTPUT"
  echo "    \"depth_results\": [" >> "$OUTPUT"

  FIRST_DEPTH=true

  # Perft loop
  for ((d=1; d<=MAX_DEPTH; d++)); do
    NODES=$(stockfish <<EOF | grep "Nodes searched" | awk '{print $3}'
uci
isready
position fen $FEN
go perft $d
quit
EOF
)

    if [ "$FIRST_DEPTH" = true ]; then
      FIRST_DEPTH=false
    else
      echo "," >> "$OUTPUT"
    fi

    echo "      { \"depth\": ${d}, \"nodes\": ${NODES} }" >> "$OUTPUT"
  done

  echo "    ]" >> "$OUTPUT"
  echo "  }" >> "$OUTPUT"

done

echo "]" >> "$OUTPUT"

echo "Saved results to $OUTPUT"
