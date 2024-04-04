import 'dart:convert';

class Vector2 {
  final num? x;
  final num? y;

  Vector2(this.x, this.y);

  factory Vector2.fromJson(Map<String, num?> json) {
    return Vector2(
      json['x'],
      json['y'],
    );
  }

  String toJson() {
    final Map<String, num?> json = {
      'x': x,
      'y': y,
    };
    return jsonEncode(json);
  }
}
