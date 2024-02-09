#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

uniform sampler2D texture0;
uniform vec4 outlineCol;

void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);

    ivec2 texSize  = textureSize(texture0, 0);
    vec2 texOffset = 1.0 / vec2(texSize.xy);

    // Get the colors of the surrounding pixels
    vec4 right = texture(texture0, fragTexCoord + texOffset * vec2(1.0, 0.0));
    vec4 left = texture(texture0, fragTexCoord + texOffset * vec2(-1.0, 0.0));
    vec4 up = texture(texture0, fragTexCoord + texOffset * vec2(0.0, 1.0));
    vec4 down = texture(texture0, fragTexCoord + texOffset * vec2(0.0, -1.0));
    
    // If we're transparent and any neighbouring pixel isn't then draw an outline
    if (texelColor.a == 0 && (right.a > 0 || left.a > 0 || up.a > 0 || down.a > 0)) {
        texelColor = outlineCol;
    }

    if (texelColor.a <= 0.0) {
        discard;
    }

    finalColor = texelColor * fragColor;
}