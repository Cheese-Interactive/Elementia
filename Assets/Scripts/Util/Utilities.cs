using System.Linq;
using UnityEngine;

public static class Utilities {

    // finds the Vector2 position of the lowest corner of the object | if there are two corners with the same lowest y value, the position in between them is returned
    public static Vector2 GetLowestCorner(SpriteRenderer spriteRenderer, float cornerLeniency) {

        Vector2 tR = spriteRenderer.transform.TransformPoint(spriteRenderer.sprite.bounds.max);
        Vector2 tL = spriteRenderer.transform.TransformPoint(new Vector2(spriteRenderer.sprite.bounds.max.x, spriteRenderer.sprite.bounds.min.y));
        Vector2 bR = spriteRenderer.transform.TransformPoint(new Vector2(spriteRenderer.sprite.bounds.min.x, spriteRenderer.sprite.bounds.max.y));
        Vector2 bL = spriteRenderer.transform.TransformPoint(spriteRenderer.sprite.bounds.min);
        Vector2[] corners = new Vector2[] { tR, tL, bR, bL };
        float lowestY = new float[] { tR.y, tL.y, bR.y, bL.y }.Min();

        Vector2[] lowestCorners = corners.Where(corner => corner.y <= lowestY + cornerLeniency && corner.y >= lowestY - cornerLeniency).ToArray(); // get all corners with the lowest y value and within the leniency range

        if (lowestCorners.Length > 1) {

            Vector2 sum = Vector2.zero;

            foreach (Vector2 corner in lowestCorners)
                sum += corner;

            return sum / lowestCorners.Length; // return the average position of the corners with the lowest y value

        } else {

            return lowestCorners[0]; // return the first corner as it is the only corner with the lowest y value

        }
    }
}
