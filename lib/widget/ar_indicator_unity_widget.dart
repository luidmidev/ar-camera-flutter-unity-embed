import 'package:camerapicoart/models/size.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_embed_unity/flutter_embed_unity.dart';
import 'package:http/http.dart' as http;

import '../models/art_data.dart';
import '../models/vector_2.dart';

const unityHttpController = "http://localhost:4444";
const onLoadScenePrefix = "@@OnLoadScene@@";

class ARIndicatorUnityWidget extends StatefulWidget {
  final ARIndicatorUnityController controller;
  final VoidCallback onSceneLoaded;
  final VoidCallback onMounted;

  static var sceneIsLoaded = false;

  const ARIndicatorUnityWidget({super.key, required this.controller, required this.onSceneLoaded, required this.onMounted});

  @override
  State<ARIndicatorUnityWidget> createState() => _ARIndicatorUnityWidgetState();
}

class _ARIndicatorUnityWidgetState extends State<ARIndicatorUnityWidget> {
  void onUnityMessage(String message) {
    if (kDebugMode) print("Received message from unity: ${message.toString()}");

    if (message.startsWith(onLoadScenePrefix)) {
      ARIndicatorUnityWidget.sceneIsLoaded = true;
      widget.onSceneLoaded();
    }

    if (message == "AR Session is tracking") {
      ARIndicatorUnityWidget.sceneIsLoaded = true;
    }
  }

  @override
  Widget build(BuildContext context) {
    return EmbedUnity(onMessageFromUnity: onUnityMessage);
  }
}

class ARIndicatorUnityController {
  bool lock = false;

  ARIndicatorUnityController();

  Future setIndicator(ArtData art) async => await _createPostRequest("set-indicator", data: art.toJsonString());

  Future rotateIndicator(num rotation) async => await _createPostRequest("rotate-indicator", data: rotation.toString());

  Future setSizeIndicator(Size size) async => await _createPostRequest("set-size-indicator", data: size.toJsonString());

  Future moveIndicator(Vector2 position) async => await _createPostRequest("move-indicator", data: position.toJson());

  Future showPlanes() async => await _createPostRequest("show-planes");

  Future hidePlanes() async => await _createPostRequest("hide-planes");

  Future clearPlanes() async => await _createPostRequest("clear-planes");

  Future<bool> statePlanes() async => await _createGetRequest("state-planes") == "True";

  Future setFactorScaleIndicator(num factor) async => await _createPostRequest("set-factor-scale-indicator", data: factor.toString());

  void pause() => pauseUnity();

  void resume() => resumeUnity();

  Future _createPostRequest(String method, {dynamic data = ""}) async {
    if (!ARIndicatorUnityWidget.sceneIsLoaded || lock) {
      if (kDebugMode) print("Returned: ${ARIndicatorUnityWidget.sceneIsLoaded} - $lock");
      return;
    }
    lock = true;
    try {
      final response = await http.post(Uri.parse("$unityHttpController/$method"), body: data);
      if (response.statusCode != 200) {
        final error = 'Failed to send message to unity with status code: ${response.statusCode} and message: ${response.body}';
        throw Exception(error);
      }
    } finally {
      lock = false;
    }
  }

  Future<String> _createGetRequest(String method) async {
    if (!ARIndicatorUnityWidget.sceneIsLoaded || lock) {
      if (kDebugMode) print("Returned: ${ARIndicatorUnityWidget.sceneIsLoaded} - $lock");
      return "";
    }
    lock = true;
    try {
      final response = await http.get(Uri.parse("$unityHttpController/$method"));
      if (response.statusCode != 200) {
        final error = 'Failed to get message from unity with status code: ${response.statusCode} and message: ${response.body}';
        throw Exception(error);
      }
      return response.body;
    } finally {
      lock = false;
    }
  }
}
