struct Input { float4 position : SV_Position; float4 color : COLOR0; };
[shader("fragment")]
float4 fs_main(Input input) : SV_Target { return input.color; }
