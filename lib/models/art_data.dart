import 'dart:convert';

import 'package:camerapicoart/models/size.dart';

class ArtData {
  final String url;
  final String token;
  final String userId;
  final int artId;
  final Size size;

  ArtData({required this.url, required this.token, required this.userId, required this.artId, required this.size});

  String toJsonString() {
    return jsonEncode(toJson());
  }

  Map<String, dynamic> toJson() {
    return {
      'url': url,
      'token': token,
      'userId': userId,
      'artId': artId,
      'size': size.toJson(),
    };
  }
}
