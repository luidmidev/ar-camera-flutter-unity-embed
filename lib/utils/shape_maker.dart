import 'package:flutter/material.dart';

enum ShapeType {
  triangle,
  octagon,
  hexagon,
  arrow,
  biArrow,
}

class ShapeMakerPainter extends CustomPainter {
  final Color strokeColor;
  final Shader? shader;
  final PaintingStyle paintingStyle;
  final MaskFilter? maskFilter;
  ShapeType shapeType = ShapeType.triangle;

  ShapeMakerPainter({
    this.shapeType = ShapeType.triangle,
    this.strokeColor = Colors.black,
    this.paintingStyle = PaintingStyle.stroke,
    this.shader,
    this.maskFilter,
  });

  @override
  void paint(Canvas canvas, Size size) {
    Paint paint = Paint()
      ..color = strokeColor
      ..shader = shader
      ..maskFilter = maskFilter
      ..style = paintingStyle;

    canvas.drawPath(getShapePath(size), paint);
  }

  Path getShapePath(Size size) {
    Path path = Path();

    switch (shapeType) {
      case ShapeType.triangle:
        path
          ..moveTo(0, size.height)
          ..lineTo(size.width / 2, 0)
          ..lineTo(size.width, size.height)
          ..lineTo(0, size.height);
        break;
      case ShapeType.octagon:
        path.moveTo(size.width * 0.7070941, size.height);
        path.lineTo(size.width * 0.2929059, size.height);
        path.lineTo(0, size.height * 0.7070941);
        path.lineTo(0, size.height * 0.2928924);
        path.lineTo(size.width * 0.2928924, 0);
        path.lineTo(size.width * 0.7071076, 0);
        path.lineTo(size.width, size.height * 0.2928924);
        path.lineTo(size.width, size.height * 0.7071076);
        path.lineTo(size.width * 0.7070941, size.height);
        path.close();
        break;
      case ShapeType.hexagon:
        path
          ..moveTo(size.width / 2, 0) // moving to topCenter 1st, then draw the path
          ..lineTo(size.width, size.height * .25)
          ..lineTo(size.width, size.height * .75)
          ..lineTo(size.width * .5, size.height)
          ..lineTo(0, size.height * .75)
          ..lineTo(0, size.height * .25)
          ..close();
        break;

      case ShapeType.arrow:
        path
          ..moveTo(size.width / 2, 0)
          ..lineTo(size.width, size.height / 2)
          ..lineTo(size.width * 0.75, size.height / 2)
          ..lineTo(size.width * 0.75, size.height)
          ..lineTo(size.width * 0.25, size.height)
          ..lineTo(size.width * 0.25, size.height / 2)
          ..lineTo(0, size.height / 2)
          ..lineTo(size.width / 2, 0);

        break;
      case ShapeType.biArrow:
        path
          ..moveTo(0, size.height / 2)
          ..lineTo(size.width / 3, 0)
          ..lineTo(size.width / 3, size.height / 4)
          ..lineTo(size.width / 3 * 2, size.height / 4)
          ..lineTo(size.width / 3 * 2, 0)
          ..lineTo(size.width, size.height / 2)
          ..lineTo(size.width / 3 * 2, size.height)
          ..lineTo(size.width / 3 * 2, size.height / 4 * 3)
          ..lineTo(size.width / 3, size.height / 4 * 3)
          ..lineTo(size.width / 3, size.height)
          ..lineTo(0, size.height / 2);

        break;
    }

    return path;
  }

  @override
  bool shouldRepaint(ShapeMakerPainter oldDelegate) {
    return oldDelegate.strokeColor != strokeColor || oldDelegate.paintingStyle != paintingStyle;
  }
}

class ShapeMaker extends StatelessWidget {
  final Widget? widget;
  final Color bgColor;
  final Shader? shader;
  final MaskFilter? maskFilter;
  final ShapeType shapeType;
  final double width;
  final double height;

  const ShapeMaker({this.width = 200, this.height = 200, this.widget, this.bgColor = Colors.black, this.shapeType = ShapeType.octagon, Key? key, this.shader, this.maskFilter})
      : super(key: key);

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
      painter: ShapeMakerPainter(
        strokeColor: bgColor,
        paintingStyle: PaintingStyle.fill,
        shapeType: shapeType,
        shader: shader,
        maskFilter: maskFilter,
      ),
      child: SizedBox(
        height: height,
        width: width,
        child: Center(child: widget),
      ),
    );
  }
}
