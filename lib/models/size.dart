import 'dart:convert';

const num defaultWidth = 0.5;

class Size {
  num? width;
  num? height;

  Size({this.width, this.height});

  String toJsonString() {
    return jsonEncode(toJson());
  }

  Map<String, num?> toJson() {
    return {
      'width': width,
      'height': height,
    };
  }

  void copyWith(Size size) {
    width = size.width;
    height = size.height;
  }
}
