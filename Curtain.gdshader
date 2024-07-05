shader_type spatial;

void vertex() {
	VERTEX.z += (cos(VERTEX.x * 16.0) + 2.0 * cos(VERTEX.x * 24.0 + 32.0));
}

void fragment() {
	// Place fragment code here.
}
