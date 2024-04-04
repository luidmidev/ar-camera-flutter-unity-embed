import 'dart:convert';


const num defaultWidth = 0.5;

class Size {
  num? width;
  num? height;

  Size({this.width, this.height});

  String toJson() {
    return jsonEncode({
      'width': width,
      'height': height,
    });
  }

  void copyWith(Size size) {
    width = size.width;
    height = size.height;
  }
}
