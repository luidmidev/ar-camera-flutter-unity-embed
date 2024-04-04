import 'dart:convert';

class ArtData {
  final String url;
  final String token;
  final String userId;
  final int artId;

  ArtData({required this.url, required this.token, required this.userId, required this.artId});

  String toJson() {
    return jsonEncode({
      'url': url,
      'token': token,
      'userId': userId,
      'artId': artId,
    });
  }
}
